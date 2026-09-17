using ContentOS.Api.Http;
using ContentOS.Application.Research.Get;
using ContentOS.Contracts.Research;

namespace ContentOS.Api.Research;

public static class GetResearchJobEndpoint
{
    public static RouteGroupBuilder MapGetResearchJob(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/{jobId:guid}",
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

                var job = result.Job;
                var response = new ResearchJobResponse(
                    job.Id,
                    job.Topic,
                    job.ScopeNotes,
                    job.Status.ToString(),
                    job.Version,
                    job.AttemptCount,
                    job.FailureReason,
                    job.OperationId,
                    job.CreatedAt,
                    job.UpdatedAt);

                return Results.Ok(response)
                    .WithETag(ETagFormatter.Format(job.Version));
            })
            .RequireAuthorization();

        return group;
    }
}
