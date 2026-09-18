namespace ContentOS.Api.Content;

public static class ContentEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapContentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var units = endpoints.MapGroup("/api/v1/content-units");
        units.MapListContentUnits();
        units.MapCreateContentUnit();
        units.MapDiscardContentUnit();

        var areas = endpoints.MapGroup("/api/v1/topic-areas");
        areas.MapListTopicAreas();

        var discoveries = endpoints.MapGroup("/api/v1/topic-discoveries");
        discoveries.MapListTopicDiscoveries();
        discoveries.MapStartTopicDiscovery();
        discoveries.MapGetTopicDiscovery();
        discoveries.MapSelectDiscoveredTopics();

        var series = endpoints.MapGroup("/api/v1/editorial-series");
        series.MapListEditorialSeries();
        series.MapCreateEditorialSeries();
        series.MapCollectEditorialSeries();
        series.MapDiscardEditorialSeries();

        var versions = endpoints.MapGroup("/api/v1/content-versions");
        versions.MapGetContentVersion();
        versions.MapUpdateContentVersion();
        versions.MapRequestContentReview();
        versions.MapApproveContentVersion();
        versions.MapRequestContentChanges();

        return endpoints;
    }
}
