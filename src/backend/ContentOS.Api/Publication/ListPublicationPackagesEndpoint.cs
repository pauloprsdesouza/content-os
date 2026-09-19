using ContentOS.Application.Publication.ListPackages;

namespace ContentOS.Api.Publication;

public static class ListPublicationPackagesEndpoint
{
    public static RouteGroupBuilder MapListPublicationPackages(this RouteGroupBuilder group)
    {
        group.MapGet(
                "/{editionId:guid}/publication-packages",
                async (
                    Guid editionId,
                    ListPublicationPackagesHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        new ListPublicationPackagesQuery(editionId),
                        cancellationToken);
                    return Results.Ok(result.Packages.Select(PublicationPackageResponseMapper.Map).ToArray());
                })
            .RequireAuthorization();

        return group;
    }
}
