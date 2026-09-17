namespace ContentOS.Application.Catalog.Ports;

public interface IEditionDetailQuery
{
    Task<EditionDetailReadModel?> GetAsync(
        Guid productId,
        Guid editionId,
        CancellationToken cancellationToken = default);
}
