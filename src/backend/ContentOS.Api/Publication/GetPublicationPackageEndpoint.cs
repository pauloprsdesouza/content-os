using ContentOS.Api.Http;
using ContentOS.Application.Publication.GetPackage;

namespace ContentOS.Api.Publication;

public static class GetPublicationPackageEndpoint
{
    public static RouteGroupBuilder MapGetPublicationPackage(this RouteGroupBuilder group)
    {
        group.MapGet(
                "/{packageId:guid}",
                async (
                    Guid packageId,
                    GetPublicationPackageHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        new GetPublicationPackageQuery(packageId),
                        cancellationToken);

                    if (!result.IsSuccess || result.Package is null)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status404NotFound,
                            title: "Publication package not found",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            });
                    }

                    return Results.Ok(PublicationPackageResponseMapper.Map(result.Package))
                        .WithETag(ETagFormatter.Format(result.Package.Version));
                })
            .RequireAuthorization();

        return group;
    }
}
