using ContentOS.Application.Knowledge.Ports;

namespace ContentOS.Application.Knowledge.Snapshots.GetSnapshots;

public sealed class GetSnapshotsHandler(
    ISourceRepository sources,
    ISnapshotsQuery snapshotsQuery)
{
    public async Task<GetSnapshotsResult> HandleAsync(
        GetSnapshotsQuery query,
        CancellationToken cancellationToken = default)
    {
        var source = await sources.GetByIdAsync(query.SourceId, cancellationToken);
        if (source is null)
        {
            return GetSnapshotsResult.NotFound();
        }

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var result = await snapshotsQuery.GetBySourceAsync(
            query.SourceId,
            page,
            pageSize,
            cancellationToken);
        return GetSnapshotsResult.Found(result);
    }
}
