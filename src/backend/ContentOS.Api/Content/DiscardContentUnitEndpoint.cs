using ContentOS.Api.Http;
using ContentOS.Application.Content.DiscardUnit;

namespace ContentOS.Api.Content;

public static class DiscardContentUnitEndpoint
{
    public static RouteGroupBuilder MapDiscardContentUnit(this RouteGroupBuilder group)
    {
        group.MapDelete(
                "/{contentUnitId:guid}",
                async (
                    Guid contentUnitId,
                    DiscardContentUnitHandler handler,
                    CancellationToken cancellationToken) =>
                    DiscardHttp.ToResult(
                        await handler.HandleAsync(
                            new DiscardContentUnitCommand(contentUnitId),
                            cancellationToken)))
            .RequireAuthorization()
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }
}
