namespace ContentOS.Application.Commerce.ReceiveKiwifyWebhook;

public interface IWebhookSignatureVerifier
{
    bool IsValid(string rawBody, string? signatureHeader, string webhookSecret);
}
