namespace ContentOS.Application.Knowledge.Ports;

public interface ISnapshotExcerptQuery
{
    Task<SnapshotExcerpt?> GetByHashAsync(
        string contentHash,
        CancellationToken cancellationToken = default);
}
