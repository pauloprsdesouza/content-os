using ContentOS.Api.Http;
using ContentOS.Application.Content.Series;
using ContentOS.Contracts.Content;

namespace ContentOS.Api.Content;

public static class CreateEditorialSeriesEndpoint
{
    public static RouteGroupBuilder MapCreateEditorialSeries(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/",
                async (
                    CreateEditorialSeriesRequest request,
                    CreateEditorialSeriesHandler handler,
                    HttpContext httpContext,
                    CancellationToken cancellationToken) =>
                {
                    if (!ContentActor.TryGetUserId(httpContext, out var userId))
                    {
                        return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthenticated");
                    }

                    var result = await handler.HandleAsync(
                        new CreateEditorialSeriesCommand(
                            request.Format,
                            request.AreaId,
                            request.AreaName,
                            request.AreaIsSubfield,
                            request.WindowDays,
                            request.Cadence,
                            userId),
                        cancellationToken);
                    if (!result.IsSuccess || result.SeriesId is null)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Series rejected",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            });
                    }

                    return Results.Created($"/api/v1/editorial-series/{result.SeriesId}", new { id = result.SeriesId });
                })
            .RequireAuthorization()
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }
}
