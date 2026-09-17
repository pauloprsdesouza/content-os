using ContentOS.Application.Commerce.Ports;
using ContentOS.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContentOS.Infrastructure.Commerce;

/// <summary>
/// Placeholder HTTP reconciler. Requires Commerce:ApiKey; full Kiwify certification is out of MVP scope.
/// </summary>
public sealed class HttpKiwifyOrderReconciler(
    IHttpClientFactory httpClientFactory,
    IOptions<CommerceOptions> options,
    ILogger<HttpKiwifyOrderReconciler> logger) : ICommerceOrderReconciler
{
    public const string HttpClientName = "kiwify";

    public Task<CommerceOrderSnapshot> GetAuthoritativeOrderAsync(
        string provider,
        string externalId,
        string? webhookPayloadJson,
        CancellationToken cancellationToken = default)
    {
        _ = provider;
        _ = webhookPayloadJson;
        _ = httpClientFactory;
        cancellationToken.ThrowIfCancellationRequested();

        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new CommerceProviderNotConfiguredException(
                "Commerce:UseStubProvider is false and Commerce:ApiKey is not configured. "
                + "Set an API key or enable the stub provider for local MVP.");
        }

        logger.LogWarning(
            "Kiwify HTTP reconciliation is not implemented for order {ExternalId}.",
            externalId);
        throw new CommerceProviderNotConfiguredException(
            "Kiwify HTTP reconciliation is not implemented in this MVP build. "
            + "Enable Commerce:UseStubProvider for local development.");
    }
}
