using ContentOS.Application.Knowledge.Ports;

namespace ContentOS.Application.UnitTests;

public sealed class FixedSnapshotExistsQuery(bool exists) : ISnapshotExistsQuery
{
    public Task<bool> ExistsAsync(Guid snapshotId, CancellationToken cancellationToken = default) =>
        Task.FromResult(exists);
}
