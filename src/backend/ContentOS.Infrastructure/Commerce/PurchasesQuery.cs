using ContentOS.Application.Commerce.Ports;
using ContentOS.Domain.Commerce;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Commerce;

public sealed class PurchasesQuery(PlatformDbContext dbContext) : IPurchasesQuery
{
    public async Task<PurchasesPage> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<Purchase>()
            .AsNoTracking()
            .OrderByDescending(purchase => purchase.CreatedAt);
        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(purchase => new PurchaseListItem(
                purchase.Id,
                purchase.Provider,
                purchase.ExternalId,
                purchase.Status.ToString(),
                purchase.BuyerEmail,
                purchase.ProductId,
                purchase.EditionId,
                purchase.LearnerId,
                purchase.WebhookInboxEntryId,
                purchase.ReconciliationRunId,
                purchase.ConfirmedAt,
                purchase.CreatedAt,
                purchase.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PurchasesPage(items, page, pageSize, totalItems);
    }
}
