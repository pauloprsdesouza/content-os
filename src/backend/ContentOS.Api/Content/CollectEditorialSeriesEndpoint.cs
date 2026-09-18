using ContentOS.Api.Http;
using ContentOS.Application.Content.Ports;
using ContentOS.Application.Content.Series;
using ContentOS.Contracts.Content;

namespace ContentOS.Api.Content;

public static class CollectEditorialSeriesEndpoint
{
    public static RouteGroupBuilder MapCollectEditorialSeries(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/{seriesId:guid}/collections",
                async (
                    Guid seriesId,
                    RunSeriesCollectionHandler handler,
                    IEditorialSeriesRepository series,
                    HttpContext httpContext,
                    TimeProvider clock,
                    CancellationToken cancellationToken) =>
                {
                    if (!ContentActor.TryGetUserId(httpContext, out var userId))
                    {
                        return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthenticated");
                    }

                    var editorial = await series.GetByIdAsync(seriesId, cancellationToken);
                    if (editorial is null || editorial.OwnerUserId != userId)
                    {
                        return Results.NotFound();
                    }

                    var result = await handler.HandleAsync(
                        new RunSeriesCollectionCommand(seriesId, clock.GetUtcNow(), Reschedule: false),
                        cancellationToken);
                    if (!result.IsSuccess || result.DiscoveryId is null)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Collection rejected",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            });
                    }

                    return Results.Ok(new StartTopicDiscoveryResponse(result.DiscoveryId.Value, null, false));
                })
            .RequireAuthorization()
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }
}
