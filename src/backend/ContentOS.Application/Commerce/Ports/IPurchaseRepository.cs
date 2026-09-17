using ContentOS.Domain.Commerce;

namespace ContentOS.Application.Commerce.Ports;

public interface IPurchaseRepository
{
    Task<Purchase?> GetByIdAsync(Guid purchaseId, CancellationToken cancellationToken = default);

    Task<Purchase?> GetByProviderExternalIdAsync(
        string provider,
        string externalId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Purchase>> ListPendingReconciliationAsync(
        CancellationToken cancellationToken = default);

    void Add(Purchase purchase);
}
