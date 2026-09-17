namespace ContentOS.Application.Commerce.Ports;

/// <summary>
/// Authoritative provider order lookup used during reconciliation (not webhook intake).
/// </summary>
public interface ICommerceOrderReconciler
{
    Task<CommerceOrderSnapshot> GetAuthoritativeOrderAsync(
        string provider,
        string externalId,
        string? webhookPayloadJson,
        CancellationToken cancellationToken = default);
}
