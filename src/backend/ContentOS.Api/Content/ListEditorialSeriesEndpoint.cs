using ContentOS.Application.Content.Ports;
using ContentOS.Contracts.Content;

namespace ContentOS.Api.Content;

public static class ListEditorialSeriesEndpoint
{
    public static RouteGroupBuilder MapListEditorialSeries(this RouteGroupBuilder group)
    {
        group.MapGet(
                "/",
                async (
                    IEditorialSeriesRepository series,
                    HttpContext httpContext,
                    CancellationToken cancellationToken) =>
                {
                    if (!ContentActor.TryGetUserId(httpContext, out var userId))
                    {
                        return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthenticated");
                    }

                    var items = await series.ListForOwnerAsync(userId, cancellationToken);
                    return Results.Ok(items.Select(item => new EditorialSeriesResponse(
                        item.Id,
                        item.Format.Code,
                        item.AreaId,
                        item.AreaName,
                        item.AreaIsSubfield,
                        item.WindowDays,
                        item.Cadence.ToString(),
                        item.NextCollectionAt,
                        item.CreatedAt)).ToArray());
                })
            .RequireAuthorization();

        return group;
    }
}
