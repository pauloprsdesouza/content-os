namespace ContentOS.Api.Content;

public static class ContentEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapContentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var units = endpoints.MapGroup("/api/v1/content-units");
        units.MapListContentUnits();
        units.MapCreateContentUnit();

        var versions = endpoints.MapGroup("/api/v1/content-versions");
        versions.MapGetContentVersion();
        versions.MapUpdateContentVersion();
        versions.MapRequestContentReview();
        versions.MapApproveContentVersion();
        versions.MapRequestContentChanges();

        return endpoints;
    }
}
