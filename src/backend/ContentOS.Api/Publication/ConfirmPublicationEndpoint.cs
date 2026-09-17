using System.Security.Claims;
using ContentOS.Api.Http;
using ContentOS.Application.Publication.ConfirmPublication;
using ContentOS.Domain.Identity;

namespace ContentOS.Api.Publication;

public static class ConfirmPublicationEndpoint
{
    public static RouteGroupBuilder MapConfirmPublication(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/{packageId:guid}/confirm-publication",
                async (
                    Guid packageId,
                    ConfirmPublicationHandler handler,
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
                            detail: "Send the publication package ETag via If-Match.",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = "PUBLICATION_PACKAGE_IF_MATCH_REQUIRED"
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
                        new ConfirmPublicationCommand(packageId, userId, expectedVersion),
                        cancellationToken);

                    if (result.IsSuccess)
                    {
                        return Results.NoContent()
                            .WithETag(ETagFormatter.Format(result.NewVersion!.Value));
                    }

                    return result.ErrorCode switch
                    {
                        "PUBLICATION_PACKAGE_NOT_FOUND" => Results.Problem(
                            statusCode: StatusCodes.Status404NotFound,
                            title: "Publication package not found",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            }),
                        "PUBLICATION_PACKAGE_STALE" => Results.Problem(
                            statusCode: StatusCodes.Status412PreconditionFailed,
                            title: "The publication package changed",
                            detail: "Reload and confirm the latest version.",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode,
                                ["currentVersion"] = result.CurrentVersion
                            }),
                        _ => Results.Problem(
                            statusCode: StatusCodes.Status409Conflict,
                            title: "Publication cannot be confirmed",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            })
                    };
                })
            .RequireAuthorization(CapabilityNames.PublicationConfirm)
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }
}
