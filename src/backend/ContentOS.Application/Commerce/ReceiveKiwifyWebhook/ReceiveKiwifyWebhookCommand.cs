namespace ContentOS.Application.Commerce.ReceiveKiwifyWebhook;

public sealed record ReceiveKiwifyWebhookCommand(
    string RawBody,
    string? SignatureHeader,
    bool RequireSignature);
