using ContentOS.Api.Http;
using ContentOS.Application.Publication.ExportPackage;

namespace ContentOS.Api.Publication;

public static class ExportPublicationPackageEndpoint
{
    public static RouteGroupBuilder MapExportPublicationPackage(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/{packageId:guid}/export",
                async (
                    Guid packageId,
                    ExportPublicationPackageHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        new ExportPublicationPackageCommand(packageId),
                        cancellationToken);

                    if (result.IsSuccess && result.Package is not null)
                    {
                        return Results.Ok(PublicationPackageResponseMapper.Map(result.Package))
                            .WithETag(ETagFormatter.Format(result.Package.Version));
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
                        _ => Results.Problem(
                            statusCode: StatusCodes.Status409Conflict,
                            title: "Publication package cannot be exported",
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
