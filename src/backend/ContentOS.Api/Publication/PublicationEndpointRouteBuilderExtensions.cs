namespace ContentOS.Api.Publication;

public static class PublicationEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapPublicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var editions = endpoints.MapGroup("/api/v1/editions");
        editions.MapCreatePublicationPackage();
        editions.MapListPublicationPackages();

        var packages = endpoints.MapGroup("/api/v1/publication-packages");
        packages.MapGetPublicationPackage();
        packages.MapExportPublicationPackage();
        packages.MapConfirmPublication();

        return endpoints;
    }
}
