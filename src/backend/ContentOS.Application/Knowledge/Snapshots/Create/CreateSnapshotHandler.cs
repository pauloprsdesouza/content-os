using ContentOS.Application.Blobs;
using ContentOS.Application.Knowledge.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Knowledge;
using ContentOS.SharedKernel;

namespace ContentOS.Application.Knowledge.Snapshots.Create;

public sealed class CreateSnapshotHandler(
    ISourceRepository sources,
    ISourceSnapshotRepository snapshots,
    IBlobStore blobs,
    IIdGenerator ids,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<CreateSnapshotResult> HandleAsync(
        CreateSnapshotCommand command,
        CancellationToken cancellationToken = default)
    {
        var source = await sources.GetByIdAsync(command.SourceId, cancellationToken);
        if (source is null)
        {
            return CreateSnapshotResult.SourceNotFound();
        }

        var blob = await blobs.PutAsync(
            command.Content,
            command.MediaType,
            cancellationToken);

        var snapshot = SourceSnapshot.Create(
            ids.NewId(),
            source.Id,
            ContentHash.Create(blob.Sha256),
            blob.MediaType,
            blob.Length,
            clock.GetUtcNow());

        snapshots.Add(snapshot);
        await changes.CommitAsync(cancellationToken);
        return CreateSnapshotResult.Created(snapshot.Id, snapshot.ContentHash.Value);
    }
}
