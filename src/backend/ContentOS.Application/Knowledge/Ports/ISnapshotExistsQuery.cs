namespace ContentOS.Application.Knowledge.Ports;

public interface ISnapshotExistsQuery
{
    Task<bool> ExistsAsync(Guid snapshotId, CancellationToken cancellationToken = default);
}
