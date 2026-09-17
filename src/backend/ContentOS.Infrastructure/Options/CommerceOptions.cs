namespace ContentOS.Infrastructure.Options;

public sealed class CommerceOptions
{
    public const string SectionName = "Commerce";

    /// <summary>
    /// When set, inbound Kiwify webhooks must present a matching HMAC-SHA256 signature.
    /// When empty, Development accepts signals and logs a warning.
    /// </summary>
    public string? WebhookSecret { get; init; }

    public string? ApiKey { get; init; }

    public string BaseUrl { get; init; } = "https://public-api.kiwify.com.br";

    /// <summary>
    /// When true, reconciliation treats the stored webhook payload as authoritative.
    /// </summary>
    public bool UseStubProvider { get; init; } = true;

    /// <summary>
    /// Fallback product when the webhook payload does not map to a catalog id.
    /// </summary>
    public Guid? DefaultProductId { get; init; }

    /// <summary>
    /// Fallback edition when the webhook payload does not map to a catalog id.
    /// </summary>
    public Guid? DefaultEditionId { get; init; }
}
