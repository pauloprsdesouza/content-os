using ContentOS.Domain.Catalog;

namespace ContentOS.Application.Catalog.Ports;

public interface IEditionRepository
{
    Task<Edition?> GetByIdAsync(
        Guid editionId,
        CancellationToken cancellationToken = default);

    Task<Edition?> GetByProductAndEditionAsync(
        Guid productId,
        Guid editionId,
        CancellationToken cancellationToken = default);

    void Add(Edition edition);
}
