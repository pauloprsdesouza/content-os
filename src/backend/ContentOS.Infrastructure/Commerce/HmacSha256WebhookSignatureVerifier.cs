using System.Security.Cryptography;
using System.Text;
using ContentOS.Application.Commerce.ReceiveKiwifyWebhook;

namespace ContentOS.Infrastructure.Commerce;

public sealed class HmacSha256WebhookSignatureVerifier : IWebhookSignatureVerifier
{
    public bool IsValid(string rawBody, string? signatureHeader, string webhookSecret)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader) || string.IsNullOrWhiteSpace(webhookSecret))
        {
            return false;
        }

        var provided = signatureHeader.Trim();
        if (provided.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
        {
            provided = provided["sha256=".Length..].Trim();
        }

        var key = Encoding.UTF8.GetBytes(webhookSecret);
        var body = Encoding.UTF8.GetBytes(rawBody);
        var hash = HMACSHA256.HashData(key, body);
        var expected = Convert.ToHexString(hash);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected.ToLowerInvariant()),
            Encoding.UTF8.GetBytes(provided.ToLowerInvariant()));
    }
}
