using ContentOS.Application.Catalog.Ports;

namespace ContentOS.Application.Catalog.ListProducts;

public sealed class ListProductsHandler(IProductsQuery productsQuery)
{
    public async Task<ListProductsResult> HandleAsync(
        ListProductsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var result = await productsQuery.GetPageAsync(page, pageSize, cancellationToken);
        return new ListProductsResult(result);
    }
}
