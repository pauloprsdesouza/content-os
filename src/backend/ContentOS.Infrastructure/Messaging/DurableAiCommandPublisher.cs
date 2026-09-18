using System.Text.Json;
using System.Text.Json.Serialization;
using ContentOS.Application.Messaging;
using ContentOS.Infrastructure.Persistence;

namespace ContentOS.Infrastructure.Messaging;

public sealed class DurableAiCommandPublisher(PlatformDbContext dbContext) : IAiCommandPublisher
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public Task PublishAsync(
        AiCloudEventEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var payload = new Dictionary<string, object?>
        {
            ["specversion"] = "1.0",
            ["id"] = envelope.Id,
            ["source"] = envelope.Source,
            ["type"] = envelope.Type,
            ["time"] = envelope.Time,
            ["subject"] = envelope.Subject,
            ["correlationid"] = envelope.CorrelationId,
            ["causationid"] = envelope.Id,
            ["idempotencykey"] = envelope.IdempotencyKey,
            ["schemaversion"] = envelope.SchemaVersion,
            ["datacontenttype"] = "application/json",
            ["data"] = envelope.Data
        };

        dbContext.Set<OutboxMessage>().Add(new OutboxMessage
        {
            Id = Guid.CreateVersion7(),
            EnvelopeId = envelope.Id,
            Type = envelope.Type,
            Subject = envelope.Subject,
            PayloadJson = JsonSerializer.Serialize(payload, SerializerOptions),
            CreatedAt = envelope.Time
        });

        return Task.CompletedTask;
    }
}
