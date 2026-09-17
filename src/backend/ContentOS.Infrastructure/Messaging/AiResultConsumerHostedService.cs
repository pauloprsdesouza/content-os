using System.Text;
using System.Text.Json;
using ContentOS.Application.Content.ApplyGenerateResult;
using ContentOS.Application.Content.ApplyReviewResult;
using ContentOS.Application.Messaging;
using ContentOS.Application.Research.ApplyResult;
using ContentOS.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ContentOS.Infrastructure.Messaging;

public sealed class AiResultConsumerHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<AiResultConsumerHostedService> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rabbit = options.Value;
        if (string.IsNullOrWhiteSpace(rabbit.Uri)
            || string.IsNullOrWhiteSpace(rabbit.AiResultsQueue))
        {
            logger.LogWarning("AI result consumer disabled; RabbitMQ Uri/queue not configured.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeLoopAsync(rabbit, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AI result consumer loop failed; retrying in 5s.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ConsumeLoopAsync(RabbitMqOptions rabbit, CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { Uri = new Uri(rabbit.Uri) };
        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: rabbit.AiResultsQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        if (!string.IsNullOrWhiteSpace(rabbit.AiRequestsExchange))
        {
            await channel.ExchangeDeclareAsync(
                exchange: rabbit.AiRequestsExchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: stoppingToken);

            await channel.QueueBindAsync(
                queue: rabbit.AiResultsQueue,
                exchange: rabbit.AiRequestsExchange,
                routingKey: "ai.#",
                cancellationToken: stoppingToken);
        }

        await channel.BasicQosAsync(0, 10, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                await HandleMessageAsync(args.Body.ToArray(), stoppingToken);
                await channel.BasicAckAsync(args.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to apply AI result; nacking without requeue.");
                await channel.BasicNackAsync(args.DeliveryTag, false, false, stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: rabbit.AiResultsQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        logger.LogInformation("AI result consumer listening on {Queue}", rabbit.AiResultsQueue);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // shut down
        }
    }

    private async Task HandleMessageAsync(byte[] body, CancellationToken cancellationToken)
    {
        using var document = JsonDocument.Parse(Encoding.UTF8.GetString(body));
        var root = document.RootElement;
        var type = root.TryGetProperty("type", out var typeNode)
            ? typeNode.GetString()
            : null;
        var idempotencyKey = root.TryGetProperty("idempotencykey", out var keyNode)
            ? keyNode.GetString() ?? string.Empty
            : string.Empty;

        if (string.IsNullOrWhiteSpace(type) || !root.TryGetProperty("data", out var data))
        {
            logger.LogWarning("Ignoring AI result without type/data.");
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var services = scope.ServiceProvider;

        switch (type)
        {
            case AiMessageTypes.ResearchCompleted:
            case AiMessageTypes.JobFailed when data.TryGetProperty("researchJobId", out _):
            {
                var researchJobId = GetGuid(data, "researchJobId");
                var operationId = GetGuid(data, "operationId");
                var succeeded = !string.Equals(type, AiMessageTypes.JobFailed, StringComparison.Ordinal);
                var findings = new List<ResearchFindingInput>();
                if (data.TryGetProperty("findings", out var findingsNode)
                    && findingsNode.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in findingsNode.EnumerateArray())
                    {
                        findings.Add(new ResearchFindingInput(
                            item.GetProperty("statement").GetString() ?? string.Empty,
                            item.TryGetProperty("confidence", out var confidence)
                                ? confidence.GetDecimal()
                                : 0.5m));
                    }
                }

                var handler = services.GetRequiredService<ApplyResearchResultHandler>();
                await handler.HandleAsync(
                    new ApplyResearchResultCommand(
                        researchJobId,
                        operationId,
                        idempotencyKey,
                        succeeded,
                        GetString(data, "errorMessage"),
                        findings),
                    cancellationToken);
                break;
            }
            case AiMessageTypes.ContentGenerateCompleted:
            {
                var handler = services.GetRequiredService<ApplyContentGenerateResultHandler>();
                await handler.HandleAsync(
                    new ApplyContentGenerateResultCommand(
                        GetGuid(data, "contentVersionId"),
                        GetGuid(data, "operationId"),
                        idempotencyKey,
                        true,
                        null,
                        GetString(data, "bodyMarkdown"),
                        data.TryGetProperty("requestReviewAfterGenerate", out var flag)
                            && flag.ValueKind == JsonValueKind.True),
                    cancellationToken);
                break;
            }
            case AiMessageTypes.ContentReviewCompleted:
            {
                var handler = services.GetRequiredService<ApplyContentReviewResultHandler>();
                await handler.HandleAsync(
                    new ApplyContentReviewResultCommand(
                        GetGuid(data, "contentVersionId"),
                        GetGuid(data, "operationId"),
                        idempotencyKey,
                        true,
                        null,
                        GetString(data, "agentReviewNotes")),
                    cancellationToken);
                break;
            }
            case AiMessageTypes.JobFailed when data.TryGetProperty("contentVersionId", out _):
            {
                var kind = GetString(data, "failedType");
                if (string.Equals(kind, AiMessageTypes.ContentReview, StringComparison.Ordinal))
                {
                    var handler = services.GetRequiredService<ApplyContentReviewResultHandler>();
                    await handler.HandleAsync(
                        new ApplyContentReviewResultCommand(
                            GetGuid(data, "contentVersionId"),
                            GetGuid(data, "operationId"),
                            idempotencyKey,
                            false,
                            GetString(data, "errorMessage"),
                            null),
                        cancellationToken);
                }
                else
                {
                    var handler = services.GetRequiredService<ApplyContentGenerateResultHandler>();
                    await handler.HandleAsync(
                        new ApplyContentGenerateResultCommand(
                            GetGuid(data, "contentVersionId"),
                            GetGuid(data, "operationId"),
                            idempotencyKey,
                            false,
                            GetString(data, "errorMessage"),
                            null,
                            false),
                        cancellationToken);
                }

                break;
            }
            default:
                logger.LogWarning("Unhandled AI result type {Type}", type);
                break;
        }
    }

    private static Guid GetGuid(JsonElement data, string name) =>
        data.TryGetProperty(name, out var node) && Guid.TryParse(node.GetString(), out var value)
            ? value
            : Guid.Empty;

    private static string? GetString(JsonElement data, string name) =>
        data.TryGetProperty(name, out var node) ? node.GetString() : null;
}
