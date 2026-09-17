namespace ContentOS.Application.Commerce.Ports;

public sealed record PurchasesPage(
    IReadOnlyList<PurchaseListItem> Items,
    int Page,
    int PageSize,
    long TotalItems);
