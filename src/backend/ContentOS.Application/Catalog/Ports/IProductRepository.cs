using ContentOS.Domain.Catalog;

namespace ContentOS.Application.Catalog.Ports;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    void Add(Product product);
}
