using ContentOS.Api.Http;
using ContentOS.Application.Knowledge.Claims.GetReviewQueue;
using ContentOS.Contracts.Knowledge;

namespace ContentOS.Api.Knowledge.Claims;

public static class GetClaimReviewQueueEndpoint
{
    public static RouteGroupBuilder MapGetClaimReviewQueue(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/",
            async (
                [AsParameters] PaginationQuery pagination,
                GetClaimReviewQueueHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new GetClaimReviewQueueQuery(pagination.Page, pagination.PageSize),
                    cancellationToken);

                var items = result.Page.Items
                    .Select(item => new ClaimQueueItemResponse(
                        item.Id,
                        item.Statement,
                        item.Status.ToString(),
                        item.Confidence,
                        item.Version,
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
