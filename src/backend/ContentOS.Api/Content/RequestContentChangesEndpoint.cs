using System.Security.Claims;
using ContentOS.Api.Http;
using ContentOS.Application.Content.RequestChanges;
using ContentOS.Contracts.Content;
using ContentOS.Domain.Identity;

namespace ContentOS.Api.Content;

public static class RequestContentChangesEndpoint
{
    public static RouteGroupBuilder MapRequestContentChanges(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/{versionId:guid}/request-changes",
                async (
                    Guid versionId,
                    RequestContentChangesRequest request,
                    RequestContentChangesHandler handler,
                    HttpContext httpContext,
                    CancellationToken cancellationToken) =>
                {
                    if (!ETagFormatter.TryParse(
                            httpContext.Request.Headers.IfMatch.ToString(),
                            out var expectedVersion))
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status428PreconditionRequired,
                            title: "If-Match required",
                            detail: "Send the content version ETag via If-Match.",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = "CONTENT_VERSION_IF_MATCH_REQUIRED"
                            });
                    }

                    var userIdValue = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!Guid.TryParse(userIdValue, out var userId))
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status401Unauthorized,
                            title: "Unauthenticated");
                    }

                    var result = await handler.HandleAsync(
                        new RequestContentChangesCommand(
                            versionId,
                            userId,
                            request.Notes,
                            expectedVersion),
                        cancellationToken);

                    if (result.IsSuccess)
                    {
                        return Results.NoContent()
                            .WithETag(ETagFormatter.Format(result.NewVersion!.Value));
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
                        "CONTENT_VERSION_STALE" => Results.Problem(
                            statusCode: StatusCodes.Status412PreconditionFailed,
                            title: "The content version changed",
                            detail: "Reload and review the latest version.",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode,
                                ["currentVersion"] = result.CurrentVersion
                            }),
                        "CONTENT_NOTES_REQUIRED" => Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Notes required",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            }),
                        _ => Results.Problem(
                            statusCode: StatusCodes.Status409Conflict,
                            title: "Content changes cannot be requested",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            })
                    };
                })
            .RequireAuthorization(CapabilityNames.ContentApprove)
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }
}
