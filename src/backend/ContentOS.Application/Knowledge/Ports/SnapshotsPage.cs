namespace ContentOS.Application.Knowledge.Ports;

public sealed record SnapshotsPage(
    IReadOnlyList<SnapshotListItem> Items,
    int Page,
    int PageSize,
    long TotalItems);
