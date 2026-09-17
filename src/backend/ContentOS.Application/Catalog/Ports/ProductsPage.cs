namespace ContentOS.Application.Catalog.Ports;

public sealed record ProductsPage(
    IReadOnlyList<ProductListItem> Items,
    int Page,
    int PageSize,
    int TotalItems);
