using ContentOS.Api.Http;
using ContentOS.Application.Catalog.ListProducts;
using ContentOS.Contracts.Catalog;

namespace ContentOS.Api.Catalog;

public static class ListProductsEndpoint
{
    public static RouteGroupBuilder MapListProducts(this RouteGroupBuilder group)
    {
        group.MapGet(
                "/",
                async (
                    [AsParameters] PaginationQuery pagination,
                    ListProductsHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        new ListProductsQuery(pagination.Page, pagination.PageSize),
                        cancellationToken);

                    var items = result.Page.Items
                        .Select(item => new ProductListItemResponse(
                            item.Id,
                            item.Name,
                            item.Description,
                            item.UpdatedAt))
                        .ToArray();

                    return Results.Ok(
                        PageResponseFactory.Create(
                            items,
                            result.Page.Page,
                            result.Page.PageSize,
                            result.Page.TotalItems));
                })
            .RequireAuthorization();

        return group;
    }
}
