using ContentOS.Application.Research.Get;
using ContentOS.Contracts.Research;

namespace ContentOS.Api.Research;

public static class GetResearchJobFindingsEndpoint
{
    public static RouteGroupBuilder MapGetResearchJobFindings(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/{jobId:guid}/claims",
            async (
                Guid jobId,
                GetResearchJobHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new GetResearchJobQuery(jobId),
                    cancellationToken);

                if (!result.IsSuccess || result.Job is null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Research job not found",
                        extensions: new Dictionary<string, object?>
                        {
                            ["code"] = "RESEARCH_JOB_NOT_FOUND"
                        });
                }

                var findings = result.Job.Findings
                    .Select(finding => new ResearchFindingResponse(
                        finding.Id,
                        finding.ResearchJobId,
                        finding.Statement,
                        finding.Confidence,
                        finding.CreatedAt))
                    .ToArray();

                return Results.Ok(findings);
            })
            .RequireAuthorization();

        return group;
    }
}
