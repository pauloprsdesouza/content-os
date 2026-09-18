using ContentOS.Domain.Knowledge;

namespace ContentOS.Application.Knowledge.Ports;

public interface IEvidenceRepository
{
    Task<IReadOnlyList<Evidence>> GetByIdsAsync(
        IReadOnlyCollection<Guid> evidenceIds,
        CancellationToken cancellationToken = default);

    void Add(Evidence evidence);

    Task<bool> AnyForSnapshotsAsync(
        IReadOnlyCollection<Guid> snapshotIds,
        CancellationToken cancellationToken = default);
}
