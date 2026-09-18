using ContentOS.Application.Knowledge.Ports;
using ContentOS.Application.Persistence;

namespace ContentOS.Application.Knowledge.DiscardSource;

public sealed class DiscardSourceHandler(
    ISourceRepository sources,
    ISourceSnapshotRepository snapshots,
    IEvidenceRepository evidence,
    IChangeCommitter changes)
{
    public async Task<DiscardOutcome> HandleAsync(
        DiscardSourceCommand command,
        CancellationToken cancellationToken = default)
    {
        var source = await sources.GetByIdAsync(command.SourceId, cancellationToken);
        if (source is null)
        {
            return DiscardOutcome.Missing("SOURCE_NOT_FOUND");
        }

        var snapshotIds = await snapshots.ListIdsBySourceAsync(command.SourceId, cancellationToken);
        if (await evidence.AnyForSnapshotsAsync(snapshotIds, cancellationToken))
        {
            return DiscardOutcome.Blocked("SOURCE_IN_USE");
        }

        await snapshots.DeleteBySourceAsync(command.SourceId, cancellationToken);
        sources.Remove(source);
        await changes.CommitAsync(cancellationToken);
        return DiscardOutcome.Ok();
    }
}
