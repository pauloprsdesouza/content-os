namespace ContentOS.Infrastructure.Messaging;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }

    public string EnvelopeId { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? SentAt { get; set; }

    public int AttemptCount { get; set; }

    public string? LastError { get; set; }
}
