using ContentOS.Application.Catalog.Ports;
using ContentOS.Domain.Catalog;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Catalog;

public sealed class EditionDetailQuery(PlatformDbContext dbContext) : IEditionDetailQuery
{
    public async Task<EditionDetailReadModel?> GetAsync(
        Guid productId,
        Guid editionId,
        CancellationToken cancellationToken = default)
    {
        var row = await (
                from edition in dbContext.Set<Edition>().AsNoTracking()
                join product in dbContext.Set<Product>().AsNoTracking()
                    on edition.ProductId equals product.Id
                where edition.Id == editionId && edition.ProductId == productId
                select new
                {
                    edition.Id,
                    edition.ProductId,
                    ProductName = product.Name,
                    edition.Name,
                    edition.Version,
                    edition.CreatedAt,
                    edition.UpdatedAt
                })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        var curriculum = await dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .Where(item => item.EditionId == editionId)
            .OrderBy(item => item.Position)
            .Select(item => new EditionCurriculumItemReadModel(
                item.Position,
                item.ContentVersionId))
            .ToListAsync(cancellationToken);

        return new EditionDetailReadModel(
            row.Id,
            row.ProductId,
            row.ProductName,
            row.Name,
            row.Version,
            curriculum,
            row.CreatedAt,
            row.UpdatedAt);
    }
}
