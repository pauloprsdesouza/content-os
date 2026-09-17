using ContentOS.Application.Catalog.Ports;
using ContentOS.Domain.Catalog;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Catalog;

public sealed class ProductRepository(PlatformDbContext dbContext) : IProductRepository
{
    public Task<Product?> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<Product>()
            .FirstOrDefaultAsync(product => product.Id == productId, cancellationToken);

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default) =>
        dbContext.Set<Product>().AnyAsync(cancellationToken);

    public void Add(Product product) => dbContext.Set<Product>().Add(product);
}
