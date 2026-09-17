namespace ContentOS.Domain.Commerce;

/// <summary>
/// Inbound provider signal only. Never treat receipt as a confirmed sale (ADR-0013).
/// </summary>
public sealed class WebhookInboxEntry
{
    private WebhookInboxEntry(
        Guid id,
        string provider,
        string externalId,
        string payloadJson,
        bool signatureVerified,
        DateTimeOffset receivedAt)
    {
        Id = id;
        Provider = provider;
        ExternalId = externalId;
        PayloadJson = payloadJson;
        SignatureVerified = signatureVerified;
        ReceivedAt = receivedAt;
    }

    private WebhookInboxEntry()
    {
        Provider = null!;
        ExternalId = null!;
        PayloadJson = null!;
    }

    public Guid Id { get; private set; }

    public string Provider { get; private set; }

    public string ExternalId { get; private set; }

    public string PayloadJson { get; private set; }

    public bool SignatureVerified { get; private set; }

    public DateTimeOffset ReceivedAt { get; private set; }

    public static WebhookInboxEntry Record(
        Guid id,
        string provider,
        string externalId,
        string payloadJson,
        bool signatureVerified,
        DateTimeOffset receivedAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Webhook inbox id is required.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId);
        ArgumentException.ThrowIfNullOrWhiteSpace(payloadJson);

        return new WebhookInboxEntry(
            id,
            provider.Trim().ToLowerInvariant(),
            externalId.Trim(),
            payloadJson,
            signatureVerified,
            receivedAt);
    }
}
