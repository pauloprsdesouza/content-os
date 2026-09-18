using ContentOS.Application.Catalog.Ports;
using ContentOS.Domain.Catalog;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Catalog;

public sealed class ProductsQuery(PlatformDbContext dbContext) : IProductsQuery
{
    public async Task<ProductsPage> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<Product>().AsNoTracking().OrderBy(product => product.Name);
        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var productIds = items.Select(product => product.Id).ToArray();
        var editions = await dbContext.Set<Edition>()
            .AsNoTracking()
            .Where(edition => productIds.Contains(edition.ProductId))
            .Select(edition => new { edition.ProductId, edition.Id, edition.CreatedAt })
            .ToListAsync(cancellationToken);
        var editionByProduct = editions
            .GroupBy(edition => edition.ProductId)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(edition => edition.CreatedAt).First().Id);

        return new ProductsPage(
            items.Select(product =>
            {
                Guid? editionId = editionByProduct.TryGetValue(product.Id, out var found) ? found : null;
                return new ProductListItem(
                    product.Id,
                    product.Name,
                    product.Description,
                    product.UpdatedAt,
                    editionId);
            }).ToList(),
            page,
            pageSize,
            totalItems);
    }
}
