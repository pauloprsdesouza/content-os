namespace ContentOS.Application.Research.Ports;

public sealed record ResearchJobsPage(
    IReadOnlyList<ResearchJobListItem> Items,
    long TotalItems);
