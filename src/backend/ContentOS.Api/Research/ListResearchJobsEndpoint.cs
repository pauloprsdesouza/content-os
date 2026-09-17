using ContentOS.Api.Http;
using ContentOS.Application.Research.List;
using ContentOS.Contracts.Research;

namespace ContentOS.Api.Research;

public static class ListResearchJobsEndpoint
{
    public static RouteGroupBuilder MapListResearchJobs(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/",
            async (
                [AsParameters] PaginationQuery pagination,
                ListResearchJobsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var page = Math.Max(1, pagination.Page);
                var pageSize = Math.Clamp(pagination.PageSize, 1, 100);

                var result = await handler.HandleAsync(
                    new ListResearchJobsQuery(page, pageSize),
                    cancellationToken);

                var items = result.Page.Items
                    .Select(item => new ResearchJobListItemResponse(
                        item.Id,
                        item.Topic,
                        item.Status,
                        item.UpdatedAt))
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
