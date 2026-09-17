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
            .Select(product => new ProductListItem(
                product.Id,
                product.Name,
                product.Description,
                product.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new ProductsPage(items, page, pageSize, totalItems);
    }
}
