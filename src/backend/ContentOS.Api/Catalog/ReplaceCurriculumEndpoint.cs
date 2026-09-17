using ContentOS.Api.Http;
using ContentOS.Application.Catalog.ReplaceCurriculum;
using ContentOS.Contracts.Catalog;

namespace ContentOS.Api.Catalog;

public static class ReplaceCurriculumEndpoint
{
    public static RouteGroupBuilder MapReplaceCurriculum(this RouteGroupBuilder group)
    {
        group.MapPut(
                "/{editionId:guid}/curriculum",
                async (
                    Guid editionId,
                    ReplaceCurriculumRequest request,
                    ReplaceCurriculumHandler handler,
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
                            detail: "Send the edition ETag via If-Match.",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = "EDITION_IF_MATCH_REQUIRED"
                            });
                    }

                    var result = await handler.HandleAsync(
                        new ReplaceCurriculumCommand(
                            editionId,
                            request.ContentVersionIds ?? [],
                            expectedVersion),
                        cancellationToken);

                    if (result.IsSuccess)
                    {
                        return Results.NoContent()
                            .WithETag(ETagFormatter.Format(result.NewVersion!.Value));
                    }

                    return result.ErrorCode switch
                    {
                        "EDITION_NOT_FOUND" => Results.Problem(
                            statusCode: StatusCodes.Status404NotFound,
                            title: "Edition not found",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            }),
                        "EDITION_STALE" => Results.Problem(
                            statusCode: StatusCodes.Status412PreconditionFailed,
                            title: "The edition changed",
                            detail: "Reload and retry with the latest ETag.",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode,
                                ["currentVersion"] = result.CurrentVersion
                            }),
                        "CURRICULUM_REQUIRES_APPROVED_VERSIONS" => Results.Problem(
                            statusCode: StatusCodes.Status422UnprocessableEntity,
                            title: "Curriculum requires approved content versions",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            }),
                        _ => Results.Problem(
                            statusCode: StatusCodes.Status409Conflict,
                            title: "Curriculum could not be updated",
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
