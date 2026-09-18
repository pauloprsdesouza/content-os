using ContentOS.Domain.Content;

namespace ContentOS.Application.Content.Ports;

public interface IContentUnitRepository
{
    Task<ContentUnit?> GetByIdAsync(
        Guid contentUnitId,
        CancellationToken cancellationToken = default);

    void Add(ContentUnit unit);

    void Remove(ContentUnit unit);
}
