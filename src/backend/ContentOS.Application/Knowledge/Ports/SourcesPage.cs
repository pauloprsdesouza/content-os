namespace ContentOS.Application.Knowledge.Ports;

public sealed record SourcesPage(
    IReadOnlyList<SourceListItem> Items,
    int Page,
    int PageSize,
    long TotalItems);
