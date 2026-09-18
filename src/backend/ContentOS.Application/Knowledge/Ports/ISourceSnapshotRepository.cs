using ContentOS.Domain.Knowledge;

namespace ContentOS.Application.Knowledge.Ports;

public interface ISourceSnapshotRepository
{
    Task<SourceSnapshot?> GetByIdAsync(
        Guid snapshotId,
        CancellationToken cancellationToken = default);

    void Add(SourceSnapshot snapshot);

    Task<IReadOnlyList<Guid>> ListIdsBySourceAsync(
        Guid sourceId,
        CancellationToken cancellationToken = default);

    Task DeleteBySourceAsync(Guid sourceId, CancellationToken cancellationToken = default);
}
