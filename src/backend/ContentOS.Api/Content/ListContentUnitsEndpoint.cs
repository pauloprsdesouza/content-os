using ContentOS.Api.Http;
using ContentOS.Application.Content.ListUnits;
using ContentOS.Contracts.Content;

namespace ContentOS.Api.Content;

public static class ListContentUnitsEndpoint
{
    public static RouteGroupBuilder MapListContentUnits(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/",
            async (
                [AsParameters] PaginationQuery pagination,
                Guid? productId,
                ListContentUnitsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var page = Math.Max(1, pagination.Page);
                var pageSize = Math.Clamp(pagination.PageSize, 1, 100);

                var result = await handler.HandleAsync(
                    new ListContentUnitsQuery(page, pageSize, productId),
                    cancellationToken);

                var items = result.Page.Items
                    .Select(item => new ContentUnitListItemResponse(
                        item.Id,
                        item.Title,
                        item.Brief,
                        item.Format,
                        item.LatestVersionId,
                        item.LatestVersionStatus,
                        item.UpdatedAt,
                        item.ProductId))
                    .ToArray();

                return Results.Ok(
                    PageResponseFactory.Create(
                        items,
                        page,
                        pageSize,
                        result.Page.TotalItems));
            })
            .RequireAuthorization();

        return group;
    }
}
