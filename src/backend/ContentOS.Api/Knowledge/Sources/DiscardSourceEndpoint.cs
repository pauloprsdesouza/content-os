using ContentOS.Api.Http;
using ContentOS.Application.Knowledge.DiscardSource;

namespace ContentOS.Api.Knowledge.Sources;

public static class DiscardSourceEndpoint
{
    public static RouteGroupBuilder MapDiscardSource(this RouteGroupBuilder group)
    {
        group.MapDelete(
                "/{sourceId:guid}",
                async (
                    Guid sourceId,
                    DiscardSourceHandler handler,
                    CancellationToken cancellationToken) =>
                    DiscardHttp.ToResult(
                        await handler.HandleAsync(new DiscardSourceCommand(sourceId), cancellationToken)))
            .RequireAuthorization()
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }
}
