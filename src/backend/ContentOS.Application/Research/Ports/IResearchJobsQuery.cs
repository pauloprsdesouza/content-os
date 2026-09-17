namespace ContentOS.Application.Research.Ports;

public interface IResearchJobsQuery
{
    Task<ResearchJobsPage> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
