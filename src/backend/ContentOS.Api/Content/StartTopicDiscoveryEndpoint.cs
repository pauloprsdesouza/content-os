using ContentOS.Api.Http;
using ContentOS.Application.Content.Topics;
using ContentOS.Contracts.Content;

namespace ContentOS.Api.Content;

public static class StartTopicDiscoveryEndpoint
{
    public static RouteGroupBuilder MapStartTopicDiscovery(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/",
                async (
                    StartTopicDiscoveryRequest request,
                    StartTopicDiscoveryHandler handler,
                    HttpContext httpContext,
                    CancellationToken cancellationToken) =>
                {
                    if (!ContentActor.TryGetUserId(httpContext, out var userId))
                    {
                        return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthenticated");
                    }

                    var result = await handler.HandleAsync(
                        new StartTopicDiscoveryCommand(
                            request.Format,
                            request.AreaId,
                            request.AreaName,
                            request.AreaIsSubfield,
                            request.WindowDays,
                            userId,
                            null,
                            null),
                        cancellationToken);
                    if (!result.IsSuccess || result.DiscoveryId is null)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Topic discovery rejected",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            });
                    }

                    return Results.Ok(new StartTopicDiscoveryResponse(
                        result.DiscoveryId.Value,
                        result.OperationId,
                        result.IsEmpty));
                })
            .RequireAuthorization()
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }
}
