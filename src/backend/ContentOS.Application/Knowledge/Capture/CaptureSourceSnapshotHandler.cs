using ContentOS.Application.Blobs;
using ContentOS.Application.Knowledge.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Knowledge;
using ContentOS.SharedKernel;

namespace ContentOS.Application.Knowledge.Capture;

public sealed class CaptureSourceSnapshotHandler(
    IEnumerable<ISourceCapture> captures,
    ISourceRepository sources,
    ISourceSnapshotRepository snapshots,
    IBlobStore blobs,
    IIdGenerator ids,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<CaptureSourceSnapshotResult> HandleAsync(
        CaptureSourceSnapshotCommand command,
        CancellationToken cancellationToken = default)
    {
        var adapter = captures.FirstOrDefault(capture => capture.Mode == command.Mode);
        if (adapter is null)
        {
            return CaptureSourceSnapshotResult.Invalid("CAPTURE_MODE_UNSUPPORTED");
        }

        var source = command.SourceId is Guid sourceId
            ? await sources.GetByIdAsync(sourceId, cancellationToken)
            : null;
        if (command.SourceId is not null && source is null)
        {
            return CaptureSourceSnapshotResult.Invalid("SOURCE_NOT_FOUND");
        }

        Uri? location = null;
        if (!string.IsNullOrWhiteSpace(command.Location))
        {
            if (!Uri.TryCreate(command.Location.Trim(), UriKind.Absolute, out location))
            {
                return CaptureSourceSnapshotResult.Invalid("SOURCE_INVALID_URI");
            }
        }
        else if (command.Mode == SourceCaptureMode.Web)
        {
            location = source?.CanonicalUri.Value;
        }

        if (source is null && location is not null)
        {
            SourceUri canonical;
            try
            {
                canonical = SourceUri.Create(location);
            }
            catch (ArgumentException)
            {
                return CaptureSourceSnapshotResult.Invalid("SOURCE_INVALID_URI");
            }

            source = await sources.GetByCanonicalUriAsync(canonical, cancellationToken);
            location = canonical.Value;
        }

        var outcome = await adapter.CaptureAsync(
            new SourceCaptureRequest(
                location,
                command.PastedText,
                command.FileContent,
                command.FileMediaType,
                command.FileName),
            cancellationToken);
        if (!outcome.IsSuccess || outcome.Content is null || outcome.MediaType is null)
        {
            return CaptureSourceSnapshotResult.Invalid(outcome.ErrorCode ?? "CAPTURE_EMPTY");
        }

        await using (outcome.Content)
        {
            var createdSource = false;
            if (source is null)
            {
                var displayName = string.IsNullOrWhiteSpace(command.DisplayName)
                    ? location?.Host
                    : command.DisplayName.Trim();
                if (string.IsNullOrWhiteSpace(displayName))
                {
                    return CaptureSourceSnapshotResult.Invalid("DISPLAY_NAME_REQUIRED");
                }

                SourceUri canonical;
                try
                {
                    canonical = location is null
                        ? SourceUri.Create($"contentos://uploads/{ids.NewId():N}")
                        : SourceUri.Create(location);
                }
                catch (ArgumentException)
                {
                    return CaptureSourceSnapshotResult.Invalid("SOURCE_INVALID_URI");
                }

                source = Source.Create(
                    ids.NewId(),
                    canonical,
                    command.Mode == SourceCaptureMode.File ? SourceKind.Upload : SourceKind.Web,
                    displayName,
                    clock.GetUtcNow());
                sources.Add(source);
                createdSource = true;
            }

            var blob = await blobs.PutAsync(outcome.Content, outcome.MediaType, cancellationToken);
            var snapshot = SourceSnapshot.Create(
                ids.NewId(),
                source.Id,
                ContentHash.Create(blob.Sha256),
                blob.MediaType,
                blob.Length,
                clock.GetUtcNow());
            snapshots.Add(snapshot);
            await changes.CommitAsync(cancellationToken);
            return CaptureSourceSnapshotResult.Created(
                source.Id,
                snapshot.Id,
                snapshot.ContentHash.Value,
                createdSource);
        }
    }
}
