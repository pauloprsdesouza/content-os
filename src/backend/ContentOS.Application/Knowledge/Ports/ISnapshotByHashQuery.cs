namespace ContentOS.Application.Knowledge.Ports;

public interface ISnapshotByHashQuery
{
    Task<SnapshotByHashDetail?> GetByHashAsync(
        string contentHash,
        CancellationToken cancellationToken = default);
}
