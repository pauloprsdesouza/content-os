using ContentOS.Api.Http;
using ContentOS.Application.Content.DiscardSeries;

namespace ContentOS.Api.Content;

public static class DiscardEditorialSeriesEndpoint
{
    public static RouteGroupBuilder MapDiscardEditorialSeries(this RouteGroupBuilder group)
    {
        group.MapDelete(
                "/{seriesId:guid}",
                async (
                    Guid seriesId,
                    DiscardEditorialSeriesHandler handler,
                    CancellationToken cancellationToken) =>
                    DiscardHttp.ToResult(
                        await handler.HandleAsync(
                            new DiscardEditorialSeriesCommand(seriesId),
                            cancellationToken)))
            .RequireAuthorization()
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }
}
