namespace ContentOS.Contracts.Dashboard;

public sealed record DashboardSummaryResponse(
    long Sources,
    long ResearchJobs,
    long ContentItems,
    long Publications);
