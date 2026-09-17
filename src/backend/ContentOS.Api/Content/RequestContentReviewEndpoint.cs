using System.Security.Claims;
using ContentOS.Api.Http;
using ContentOS.Application.Content.RequestReview;
using ContentOS.Contracts.Common;

namespace ContentOS.Api.Content;

public static class RequestContentReviewEndpoint
{
    public static RouteGroupBuilder MapRequestContentReview(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/{versionId:guid}/request-review",
                async (
                    Guid versionId,
                    RequestContentReviewHandler handler,
                    HttpContext httpContext,
                    CancellationToken cancellationToken) =>
                {
                    var userIdValue = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!Guid.TryParse(userIdValue, out var userId))
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status401Unauthorized,
                            title: "Unauthenticated");
                    }

                    var result = await handler.HandleAsync(
                        new RequestContentReviewCommand(versionId, userId),
                        cancellationToken);

                    if (result.IsSuccess)
                    {
                        var operationId = result.OperationId!.Value;
                        var statusUrl = $"/api/v1/operations/{operationId}";
                        return Results.Accepted(
                            statusUrl,
                            new OperationAcceptedResponse(
                                operationId,
                                statusUrl,
                                versionId));
                    }

                    return result.ErrorCode switch
                    {
                        "CONTENT_VERSION_NOT_FOUND" => Results.Problem(
                            statusCode: StatusCodes.Status404NotFound,
                            title: "Content version not found",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            }),
                        _ => Results.Problem(
                            statusCode: StatusCodes.Status409Conflict,
                            title: "Content review cannot be requested",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            })
                    };
                })
            .RequireAuthorization()
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }
}
