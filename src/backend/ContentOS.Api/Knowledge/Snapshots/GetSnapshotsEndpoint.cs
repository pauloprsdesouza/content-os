using ContentOS.Api.Http;
using ContentOS.Application.Knowledge.Snapshots.GetSnapshots;
using ContentOS.Contracts.Knowledge;

namespace ContentOS.Api.Knowledge.Snapshots;

public static class GetSnapshotsEndpoint
{
    public static RouteGroupBuilder MapGetSnapshots(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/{sourceId:guid}/snapshots",
            async (
                Guid sourceId,
                [AsParameters] PaginationQuery pagination,
                GetSnapshotsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new GetSnapshotsQuery(sourceId, pagination.Page, pagination.PageSize),
                    cancellationToken);

                if (!result.SourceFound || result.Page is null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Source not found",
                        extensions: new Dictionary<string, object?>
                        {
                            ["code"] = "SOURCE_NOT_FOUND"
                        });
                }

                var items = result.Page.Items
                    .Select(item => new SnapshotResponse(
                        item.Id,
                        item.SourceId,
                        item.ContentHash,
                        item.MediaType,
                        item.ByteLength,
                        item.CapturedAt))
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
