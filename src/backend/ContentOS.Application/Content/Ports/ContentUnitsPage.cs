namespace ContentOS.Application.Content.Ports;

public sealed record ContentUnitsPage(
    IReadOnlyList<ContentUnitListItem> Items,
    long TotalItems);
