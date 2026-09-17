using ContentOS.Application.Research.Ports;

namespace ContentOS.Application.Research.Get;

public sealed class GetResearchJobHandler(IResearchJobRepository jobs)
{
    public async Task<GetResearchJobResult> HandleAsync(
        GetResearchJobQuery query,
        CancellationToken cancellationToken = default)
    {
        var job = await jobs.GetByIdAsync(query.ResearchJobId, cancellationToken);
        return job is null
            ? GetResearchJobResult.NotFound()
            : GetResearchJobResult.Found(job);
    }
}
