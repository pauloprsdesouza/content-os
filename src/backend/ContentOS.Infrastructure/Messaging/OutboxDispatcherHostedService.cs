using ContentOS.Infrastructure.Options;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContentOS.Infrastructure.Messaging;

public sealed class OutboxDispatcherHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<OutboxDispatcherHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(options.Value.Uri))
        {
            logger.LogInformation("Outbox dispatcher idle; RabbitMQ is disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchBatchAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "Outbox dispatch batch failed; messages remain durable.");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task DispatchBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var transport = scope.ServiceProvider.GetRequiredService<RabbitMqAiCommandPublisher>();

        var pending = await db.Set<OutboxMessage>()
            .Where(message => message.SentAt == null && message.AttemptCount < 8)
            .OrderBy(message => message.CreatedAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in pending)
        {
            try
            {
                await transport.PublishRawAsync(
                    message.Type,
                    message.EnvelopeId,
                    message.PayloadJson,
                    cancellationToken);
                message.SentAt = DateTimeOffset.UtcNow;
                message.LastError = null;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                message.AttemptCount++;
                message.LastError = exception.GetType().Name;
                logger.LogWarning(
                    "Outbox message {EnvelopeId} attempt {Attempt} failed with {Error}",
                    message.EnvelopeId,
                    message.AttemptCount,
                    exception.GetType().Name);
            }
        }

        if (pending.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
