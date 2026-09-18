using ContentOS.Api.Http;
using ContentOS.Application.Content.Topics;
using ContentOS.Contracts.Common;
using ContentOS.Contracts.Content;

namespace ContentOS.Api.Content;

public static class SelectDiscoveredTopicsEndpoint
{
    public static RouteGroupBuilder MapSelectDiscoveredTopics(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/{discoveryId:guid}/selection",
                async (
                    Guid discoveryId,
                    SelectDiscoveredTopicsRequest request,
                    SelectDiscoveredTopicsHandler handler,
                    HttpContext httpContext,
                    CancellationToken cancellationToken) =>
                {
                    if (!ContentActor.TryGetUserId(httpContext, out var userId))
                    {
                        return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthenticated");
                    }

                    var result = await handler.HandleAsync(
                        new SelectDiscoveredTopicsCommand(
                            discoveryId,
                            request.ProposalIds,
                            userId,
                            request.ProductId),
                        cancellationToken);
                    if (!result.IsSuccess || result.OperationId is null || result.ContentUnitId is null)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Topic selection rejected",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            });
                    }

                    var statusUrl = $"/api/v1/operations/{result.OperationId}";
                    return Results.Accepted(
                        statusUrl,
                        new OperationAcceptedResponse(result.OperationId.Value, statusUrl, result.ContentUnitId));
                })
            .RequireAuthorization()
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }
}
