using ContentOS.Api.Http;
using ContentOS.Application.Publication.CreatePackage;

namespace ContentOS.Api.Publication;

public static class CreatePublicationPackageEndpoint
{
    public static RouteGroupBuilder MapCreatePublicationPackage(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/{editionId:guid}/publication-packages",
                async (
                    Guid editionId,
                    CreatePublicationPackageHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        new CreatePublicationPackageCommand(editionId),
                        cancellationToken);

                    if (result.IsSuccess && result.Package is not null)
                    {
                        var response = PublicationPackageResponseMapper.Map(result.Package);
                        return Results.Created(
                                $"/api/v1/publication-packages/{result.Package.Id:D}",
                                response)
                            .WithETag(ETagFormatter.Format(result.Package.Version));
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
                        "CURRICULUM_EMPTY" => Results.Problem(
                            statusCode: StatusCodes.Status422UnprocessableEntity,
                            title: "Edition curriculum is empty",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
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
                            title: "Publication package could not be created",
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
