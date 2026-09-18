namespace ContentOS.Application.Dashboard;

public interface IDashboardSummaryQuery
{
    Task<DashboardSummary> GetAsync(CancellationToken cancellationToken = default);
}
