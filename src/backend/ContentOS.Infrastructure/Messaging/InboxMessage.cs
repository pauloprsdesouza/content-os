namespace ContentOS.Infrastructure.Messaging;

public sealed class InboxMessage
{
    public Guid Id { get; set; }

    public string MessageId { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public DateTimeOffset ProcessedAt { get; set; }
}
