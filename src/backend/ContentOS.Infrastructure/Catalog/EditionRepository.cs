using ContentOS.Application.Catalog.Ports;
using ContentOS.Domain.Catalog;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Catalog;

public sealed class EditionRepository(PlatformDbContext dbContext) : IEditionRepository
{
    public Task<Edition?> GetByIdAsync(
        Guid editionId,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<Edition>()
            .Include(edition => edition.Curriculum)
            .FirstOrDefaultAsync(edition => edition.Id == editionId, cancellationToken);

    public Task<Edition?> GetByProductAndEditionAsync(
        Guid productId,
        Guid editionId,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<Edition>()
            .Include(edition => edition.Curriculum)
            .FirstOrDefaultAsync(
                edition => edition.Id == editionId && edition.ProductId == productId,
                cancellationToken);

    public void Add(Edition edition) => dbContext.Set<Edition>().Add(edition);

    public async Task<IReadOnlyList<Edition>> ListByProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default) =>
        await dbContext.Set<Edition>()
            .Include(edition => edition.Curriculum)
            .Where(edition => edition.ProductId == productId)
            .ToListAsync(cancellationToken);

    public void Remove(Edition edition) => dbContext.Set<Edition>().Remove(edition);
}
