namespace ContentOS.Application.Knowledge.Ports;

public interface ISnapshotsQuery
{
    Task<SnapshotsPage> GetBySourceAsync(
        Guid sourceId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
