namespace ContentOS.Application.Commerce.Ports;

public interface IPurchasesQuery
{
    Task<PurchasesPage> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
