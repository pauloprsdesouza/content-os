namespace ContentOS.Api.Learning;

public static class LearningEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapLearningEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var outcomes = endpoints.MapGroup("/api/v1/outcomes");
        outcomes.MapGetOutcomesSummary();

        var enrollments = endpoints.MapGroup("/api/v1/enrollments");
        enrollments.MapSubmitCapstone();

        var capstones = endpoints.MapGroup("/api/v1/capstones");
        capstones.MapEvaluateCapstone();

        return endpoints;
    }
}
