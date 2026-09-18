namespace ContentOS.Api.Research;

public static class ResearchEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapResearchEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var jobs = endpoints.MapGroup("/api/v1/research-jobs");
        jobs.MapListResearchJobs();
        jobs.MapCreateResearchJob();
        jobs.MapGetResearchJob();
        jobs.MapGetResearchJobFindings();
        jobs.MapDiscardResearchJob();

        return endpoints;
    }
}
