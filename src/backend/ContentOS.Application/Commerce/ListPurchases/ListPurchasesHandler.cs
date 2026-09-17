using ContentOS.Application.Commerce.Ports;

namespace ContentOS.Application.Commerce.ListPurchases;

public sealed class ListPurchasesHandler(IPurchasesQuery purchasesQuery)
{
    public async Task<ListPurchasesResult> HandleAsync(
        ListPurchasesQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var result = await purchasesQuery.GetPageAsync(page, pageSize, cancellationToken);
        return new ListPurchasesResult(result);
    }
}
