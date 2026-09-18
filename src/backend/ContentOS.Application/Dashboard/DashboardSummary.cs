namespace ContentOS.Application.Dashboard;

public sealed record DashboardSummary(
    long SourcesTotal,
    long ClaimsPendingReview,
    long ResearchJobsActive,
    long ContentVersionsPendingApproval,
    long PublicationPackagesReady,
    long PurchasesConfirmed,
    long LearnersActive,
    long OutcomesCompleted,
    DateTimeOffset GeneratedAt);
