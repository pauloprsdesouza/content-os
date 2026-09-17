using ContentOS.Application.Commerce.Ports;
using ContentOS.Domain.Commerce;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Commerce;

public sealed class PurchaseRepository(PlatformDbContext dbContext) : IPurchaseRepository
{
    public Task<Purchase?> GetByIdAsync(
        Guid purchaseId,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<Purchase>()
            .FirstOrDefaultAsync(purchase => purchase.Id == purchaseId, cancellationToken);

    public Task<Purchase?> GetByProviderExternalIdAsync(
        string provider,
        string externalId,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<Purchase>()
            .FirstOrDefaultAsync(
                purchase => purchase.Provider == provider && purchase.ExternalId == externalId,
                cancellationToken);

    public async Task<IReadOnlyList<Purchase>> ListPendingReconciliationAsync(
        CancellationToken cancellationToken = default) =>
        await dbContext.Set<Purchase>()
            .Where(purchase => purchase.Status == PurchaseStatus.SignalReceived)
            .OrderBy(purchase => purchase.CreatedAt)
            .ToListAsync(cancellationToken);

    public void Add(Purchase purchase) =>
        dbContext.Set<Purchase>().Add(purchase);
}
