using ContentOS.Application.Content.Ports;
using ContentOS.Contracts.Content;
using ContentOS.Domain.Content;

namespace ContentOS.Api.Content;

public static class ListTopicDiscoveriesEndpoint
{
    public static RouteGroupBuilder MapListTopicDiscoveries(this RouteGroupBuilder group)
    {
        group.MapGet(
                "/",
                async (
                    string? status,
                    ITopicDiscoveryRepository discoveries,
                    HttpContext httpContext,
                    CancellationToken cancellationToken) =>
                {
                    if (!ContentActor.TryGetUserId(httpContext, out var userId))
                    {
                        return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthenticated");
                    }

                    TopicDiscoveryStatus? parsed = Enum.TryParse<TopicDiscoveryStatus>(status, out var value)
                        ? value
                        : null;
                    var items = await discoveries.ListForOwnerAsync(userId, parsed, cancellationToken);
                    var response = new List<TopicDiscoveryResponse>();
                    foreach (var discovery in items)
                    {
                        var proposals = await discoveries.ListProposalsAsync(discovery.Id, cancellationToken);
                        response.Add(new TopicDiscoveryResponse(
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
                    }

                    return Results.Ok(response);
                })
            .RequireAuthorization();

        return group;
    }
}
