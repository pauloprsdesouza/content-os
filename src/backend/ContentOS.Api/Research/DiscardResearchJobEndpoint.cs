using ContentOS.Api.Http;
using ContentOS.Application.Research.DiscardJob;

namespace ContentOS.Api.Research;

public static class DiscardResearchJobEndpoint
{
    public static RouteGroupBuilder MapDiscardResearchJob(this RouteGroupBuilder group)
    {
        group.MapDelete(
                "/{researchJobId:guid}",
                async (
                    Guid researchJobId,
                    DiscardResearchJobHandler handler,
                    CancellationToken cancellationToken) =>
                    DiscardHttp.ToResult(
                        await handler.HandleAsync(
                            new DiscardResearchJobCommand(researchJobId),
                            cancellationToken)))
            .RequireAuthorization()
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }
}
