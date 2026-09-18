using System.Text;
using System.Text.Json;
using ContentOS.Application.Content.ApplyGenerateResult;
using ContentOS.Application.Content.ApplyReviewResult;
using ContentOS.Application.Content.Topics;
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

        var deadLetterQueue = string.IsNullOrWhiteSpace(rabbit.AiResultsDeadLetterQueue)
            ? $"{rabbit.AiResultsQueue}.dlq"
            : rabbit.AiResultsDeadLetterQueue;
        await channel.QueueDeclareAsync(
            queue: deadLetterQueue,
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
                var disposition = await HandleMessageAsync(args.Body.ToArray(), stoppingToken);
                if (disposition == AiResultDisposition.DeadLetter)
                {
                    await PublishDeadLetterAsync(channel, deadLetterQueue, args.Body.ToArray(), stoppingToken);
                }

                if (disposition == AiResultDisposition.Retry && !args.Redelivered)
                {
                    await channel.BasicNackAsync(args.DeliveryTag, false, true, stoppingToken);
                    return;
                }

                if (disposition == AiResultDisposition.Retry)
                {
                    await PublishDeadLetterAsync(channel, deadLetterQueue, args.Body.ToArray(), stoppingToken);
                }

                await channel.BasicAckAsync(args.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to apply AI result.");
                if (args.Redelivered)
                {
                    await PublishDeadLetterAsync(channel, deadLetterQueue, args.Body.ToArray(), stoppingToken);
                    await channel.BasicAckAsync(args.DeliveryTag, false, stoppingToken);
                    return;
                }

                await channel.BasicNackAsync(args.DeliveryTag, false, true, stoppingToken);
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

    private async Task<AiResultDisposition> HandleMessageAsync(byte[] body, CancellationToken cancellationToken)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(Encoding.UTF8.GetString(body));
        }
        catch (JsonException)
        {
            logger.LogWarning("AI result payload is not JSON; sending to dead-letter.");
            return AiResultDisposition.DeadLetter;
        }

        using (document)
        {
        var root = document.RootElement;
        var type = root.TryGetProperty("type", out var typeNode)
            ? typeNode.GetString()
            : null;
        var messageId = root.TryGetProperty("id", out var idNode)
            ? idNode.GetString()
            : null;
        var idempotencyKey = root.TryGetProperty("idempotencykey", out var keyNode)
            ? keyNode.GetString() ?? string.Empty
            : string.Empty;

        if (string.IsNullOrWhiteSpace(type) || !root.TryGetProperty("data", out var data))
        {
            logger.LogWarning("AI result missing type or data; sending to dead-letter.");
            return AiResultDisposition.DeadLetter;
        }

        if (string.IsNullOrWhiteSpace(messageId))
        {
            messageId = idempotencyKey;
        }

        if (string.IsNullOrWhiteSpace(messageId))
        {
            logger.LogWarning("AI result missing id; sending to dead-letter.");
            return AiResultDisposition.DeadLetter;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var inbox = services.GetRequiredService<ProcessedAiResultStore>();
        if (await inbox.ExistsAsync(messageId, cancellationToken))
        {
            logger.LogInformation("Duplicate AI result {MessageId} ignored.", messageId);
            return AiResultDisposition.Ack;
        }

        inbox.Remember(messageId, type);

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
                        Guid? snapshotId = null;
                        if (item.TryGetProperty("sourceSnapshotId", out var snapshotNode)
                            && Guid.TryParse(snapshotNode.GetString(), out var parsedSnapshot))
                        {
                            snapshotId = parsedSnapshot;
                        }

                        Guid? findingId = null;
                        if (item.TryGetProperty("findingId", out var findingNode)
                            && Guid.TryParse(findingNode.GetString(), out var parsedFinding))
                        {
                            findingId = parsedFinding;
                        }

                        findings.Add(new ResearchFindingInput(
                            item.GetProperty("statement").GetString() ?? string.Empty,
                            item.TryGetProperty("confidence", out var confidence)
                                ? confidence.GetDecimal()
                                : 0.5m,
                            snapshotId,
                            item.TryGetProperty("locator", out var locatorNode) ? locatorNode.GetString() : null,
                            item.TryGetProperty("extractionMethod", out var methodNode) ? methodNode.GetString() : null,
                            findingId));
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
            case AiMessageTypes.TopicLabelCompleted:
            case AiMessageTypes.JobFailed when HasDiscovery(data) && !HasContentVersion(data):
            {
                var topics = new List<TopicLabelInput>();
                if (data.TryGetProperty("topics", out var topicsNode) && topicsNode.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in topicsNode.EnumerateArray())
                    {
                        var workIds = new List<string>();
                        if (item.TryGetProperty("workIds", out var idsNode) && idsNode.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var workNode in idsNode.EnumerateArray())
                            {
                                var workId = workNode.GetString();
                                if (!string.IsNullOrWhiteSpace(workId))
                                {
                                    workIds.Add(workId);
                                }
                            }
                        }

                        topics.Add(new TopicLabelInput(
                            item.TryGetProperty("label", out var labelNode) ? labelNode.GetString() ?? string.Empty : string.Empty,
                            item.TryGetProperty("rationale", out var rationaleNode) ? rationaleNode.GetString() : null,
                            workIds));
                    }
                }

                var handler = services.GetRequiredService<ApplyTopicLabelsHandler>();
                await handler.HandleAsync(
                    new ApplyTopicLabelsCommand(
                        GetGuid(data, "discoveryId"),
                        GetGuid(data, "operationId"),
                        !string.Equals(type, AiMessageTypes.JobFailed, StringComparison.Ordinal),
                        GetString(data, "errorMessage"),
                        topics),
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
            case AiMessageTypes.Research:
            case AiMessageTypes.ContentGenerate:
            case AiMessageTypes.ContentReview:
            case AiMessageTypes.TopicLabel:
                return AiResultDisposition.Ack;
            default:
                logger.LogWarning("Unhandled AI result type {Type}", type);
                return AiResultDisposition.DeadLetter;
        }

        return AiResultDisposition.Ack;
        }
    }

    private async Task PublishDeadLetterAsync(
        IChannel channel,
        string queue,
        byte[] body,
        CancellationToken cancellationToken)
    {
        logger.LogWarning("AI result moved to dead-letter queue {Queue}. Body is not logged.", queue);
        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queue,
            mandatory: false,
            basicProperties: new BasicProperties { DeliveryMode = DeliveryModes.Persistent },
            body: body,
            cancellationToken: cancellationToken);
    }

    private static bool HasDiscovery(JsonElement data) =>
        data.TryGetProperty("discoveryId", out var node)
        && node.ValueKind == JsonValueKind.String
        && Guid.TryParse(node.GetString(), out _);

    private static bool HasContentVersion(JsonElement data) =>
        data.TryGetProperty("contentVersionId", out var node) && node.ValueKind != JsonValueKind.Null;

    private static Guid GetGuid(JsonElement data, string name) =>
        data.TryGetProperty(name, out var node) && Guid.TryParse(node.GetString(), out var value)
            ? value
            : Guid.Empty;

    private static string? GetString(JsonElement data, string name) =>
        data.TryGetProperty(name, out var node) ? node.GetString() : null;
}
