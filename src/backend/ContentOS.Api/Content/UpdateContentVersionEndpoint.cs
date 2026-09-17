using ContentOS.Api.Http;
using ContentOS.Application.Content.UpdateVersion;
using ContentOS.Contracts.Content;

namespace ContentOS.Api.Content;

public static class UpdateContentVersionEndpoint
{
    public static RouteGroupBuilder MapUpdateContentVersion(this RouteGroupBuilder group)
    {
        group.MapPut(
                "/{versionId:guid}",
                async (
                    Guid versionId,
                    UpdateContentVersionRequest request,
                    UpdateContentVersionHandler handler,
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

                    var result = await handler.HandleAsync(
                        new UpdateContentVersionCommand(
                            versionId,
                            request.BodyMarkdown,
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
                        "CONTENT_VERSION_IMMUTABLE" => Results.Problem(
                            statusCode: StatusCodes.Status409Conflict,
                            title: "Content version is immutable",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            }),
                        "CONTENT_BODY_REQUIRED" => Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Body required",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            }),
                        _ => Results.Problem(
                            statusCode: StatusCodes.Status409Conflict,
                            title: "Content version cannot be updated",
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
