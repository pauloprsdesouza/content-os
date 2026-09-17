namespace ContentOS.Application.Messaging;

public sealed record AiCloudEventEnvelope(
    string Id,
    string Source,
    string Type,
    DateTimeOffset Time,
    string Subject,
    string? CorrelationId,
    string IdempotencyKey,
    string SchemaVersion,
    object Data);
