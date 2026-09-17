using ContentOS.Domain.Content;

namespace ContentOS.Application.Content.Ports;

public interface IContentVersionRepository
{
    Task<ContentVersion?> GetByIdAsync(
        Guid contentVersionId,
        CancellationToken cancellationToken = default);

    Task<int> GetNextRevisionAsync(
        Guid contentUnitId,
        CancellationToken cancellationToken = default);

    void Add(ContentVersion version);
}
