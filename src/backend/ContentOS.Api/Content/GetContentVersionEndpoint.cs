using ContentOS.Api.Http;
using ContentOS.Application.Content.GetVersion;
using ContentOS.Contracts.Content;

namespace ContentOS.Api.Content;

public static class GetContentVersionEndpoint
{
    public static RouteGroupBuilder MapGetContentVersion(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/{versionId:guid}",
            async (
                Guid versionId,
                GetContentVersionHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new GetContentVersionQuery(versionId),
                    cancellationToken);

                if (!result.IsSuccess || result.Version is null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Content version not found",
                        extensions: new Dictionary<string, object?>
                        {
                            ["code"] = "CONTENT_VERSION_NOT_FOUND"
                        });
                }

                var version = result.Version;
                var response = new ContentVersionResponse(
                    version.Id,
                    version.ContentUnitId,
                    version.Revision,
                    version.BodyMarkdown,
                    version.Status.ToString(),
                    version.Version,
                    version.AgentReviewNotes,
                    version.ChangeRequestNotes,
                    version.ReviewedByUserId,
                    version.ReviewedAt,
                    version.OperationId,
                    version.CreatedAt,
                    version.UpdatedAt);

                return Results.Ok(response)
                    .WithETag(ETagFormatter.Format(version.Version));
            })
            .RequireAuthorization();

        return group;
    }
}
