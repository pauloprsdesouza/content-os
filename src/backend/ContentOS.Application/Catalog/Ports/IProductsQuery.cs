namespace ContentOS.Application.Catalog.Ports;

public interface IProductsQuery
{
    Task<ProductsPage> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
