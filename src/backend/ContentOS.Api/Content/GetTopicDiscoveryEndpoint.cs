using ContentOS.Application.Content.Ports;
using ContentOS.Contracts.Content;

namespace ContentOS.Api.Content;

public static class GetTopicDiscoveryEndpoint
{
    public static RouteGroupBuilder MapGetTopicDiscovery(this RouteGroupBuilder group)
    {
        group.MapGet(
                "/{discoveryId:guid}",
                async (
                    Guid discoveryId,
                    ITopicDiscoveryRepository discoveries,
                    HttpContext httpContext,
                    CancellationToken cancellationToken) =>
                {
                    if (!ContentActor.TryGetUserId(httpContext, out var userId))
                    {
                        return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthenticated");
                    }

                    var discovery = await discoveries.GetByIdAsync(discoveryId, cancellationToken);
                    if (discovery is null || discovery.OwnerUserId != userId)
                    {
                        return Results.NotFound();
                    }

                    var proposals = await discoveries.ListProposalsAsync(discoveryId, cancellationToken);
                    return Results.Ok(new TopicDiscoveryResponse(
                        discovery.Id,
                        discovery.Format.Code,
                        discovery.AreaId,
                        discovery.AreaName,
                        discovery.AreaIsSubfield,
                        discovery.WindowDays,
                        discovery.Status.ToString(),
                        discovery.OperationId,
                        discovery.ContentUnitId,
                        discovery.SeriesId,
                        proposals.Select(proposal => new TopicProposalResponse(
                            proposal.Id,
                            proposal.Label,
                            proposal.Rationale,
                            proposal.ListWorkIds(),
                            proposal.IsSelected)).ToArray()));
                })
            .RequireAuthorization();

        return group;
    }
}
