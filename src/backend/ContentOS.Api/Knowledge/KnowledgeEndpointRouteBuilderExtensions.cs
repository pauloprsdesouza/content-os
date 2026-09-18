using ContentOS.Api.Knowledge.Claims;
using ContentOS.Api.Knowledge.Snapshots;
using ContentOS.Api.Knowledge.Sources;

namespace ContentOS.Api.Knowledge;

public static class KnowledgeEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapKnowledgeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var sources = endpoints.MapGroup("/api/v1/sources");
        sources.MapGetSources();
        sources.MapCreateSource();
        sources.MapGetSource();
        sources.MapDiscardSource();
        sources.MapGetSnapshots();
        sources.MapCreateSnapshot();
        sources.MapCaptureExistingSource();

        endpoints.MapGroup("/api/v1").MapCreateSourceCapture();

        var claims = endpoints.MapGroup("/api/v1/claims");
        claims.MapGetClaimReviewQueue();
        claims.MapCreateClaimForReview();
        claims.MapGetClaim();
        claims.MapApproveClaim();
        claims.MapRejectClaim();

        return endpoints;
    }
}
