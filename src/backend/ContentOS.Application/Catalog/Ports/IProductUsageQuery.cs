namespace ContentOS.Application.Catalog.Ports;

public interface IProductUsageQuery
{
    Task<bool> IsReferencedAsync(
        Guid productId,
        IReadOnlyCollection<Guid> editionIds,
        CancellationToken cancellationToken = default);
}
