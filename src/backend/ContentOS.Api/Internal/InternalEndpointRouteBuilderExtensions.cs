namespace ContentOS.Api.Internal;

public static class InternalEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapInternalEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var internalApi = endpoints.MapGroup("/api/v1/internal");
        internalApi.MapGetSnapshotByHash();
        internalApi.MapGetSnapshotExcerpt();

        return endpoints;
    }
}
