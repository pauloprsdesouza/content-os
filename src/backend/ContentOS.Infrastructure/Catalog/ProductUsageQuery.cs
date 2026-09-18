using ContentOS.Application.Catalog.Ports;
using ContentOS.Domain.Commerce;
using ContentOS.Domain.Learning;
using ContentOS.Domain.Publication;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Catalog;

public sealed class ProductUsageQuery(PlatformDbContext dbContext) : IProductUsageQuery
{
    public async Task<bool> IsReferencedAsync(
        Guid productId,
        IReadOnlyCollection<Guid> editionIds,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Set<Purchase>()
                .AsNoTracking()
                .AnyAsync(
                    purchase => purchase.ProductId == productId
                        || (purchase.EditionId != null && editionIds.Contains(purchase.EditionId.Value)),
                    cancellationToken))
        {
            return true;
        }

        if (await dbContext.Set<Enrollment>()
                .AsNoTracking()
                .AnyAsync(enrollment => enrollment.ProductId == productId, cancellationToken))
        {
            return true;
        }

        return await dbContext.Set<PublicationPackage>()
            .AsNoTracking()
            .AnyAsync(package => editionIds.Contains(package.EditionId), cancellationToken);
    }
}
