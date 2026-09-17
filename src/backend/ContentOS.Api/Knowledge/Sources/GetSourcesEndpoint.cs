using ContentOS.Api.Http;
using ContentOS.Application.Knowledge.Sources.GetSources;
using ContentOS.Contracts.Knowledge;

namespace ContentOS.Api.Knowledge.Sources;

public static class GetSourcesEndpoint
{
    public static RouteGroupBuilder MapGetSources(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/",
            async (
                [AsParameters] PaginationQuery pagination,
                GetSourcesHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new GetSourcesQuery(pagination.Page, pagination.PageSize),
                    cancellationToken);

                var items = result.Page.Items
                    .Select(item => new SourceListItemResponse(
                        item.Id,
                        item.DisplayName,
                        item.CanonicalUri,
                        item.Kind.ToString(),
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
