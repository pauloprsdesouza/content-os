using ContentOS.Application.Content.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Content;

namespace ContentOS.Application.Content.RequestChanges;

public sealed class RequestContentChangesHandler(
    IContentVersionRepository versions,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<RequestContentChangesResult> HandleAsync(
        RequestContentChangesCommand command,
        CancellationToken cancellationToken = default)
    {
        var version = await versions.GetByIdAsync(command.ContentVersionId, cancellationToken);
        if (version is null)
        {
            return RequestContentChangesResult.NotFound();
        }

        if (version.Version != command.ExpectedVersion)
        {
            return RequestContentChangesResult.ConcurrencyConflict(version.Version);
        }

        try
        {
            version.RequestChanges(
                command.ActorUserId,
                command.Notes,
                command.ExpectedVersion,
                clock.GetUtcNow());
        }
        catch (ArgumentException)
        {
            return RequestContentChangesResult.InvalidState("CONTENT_NOTES_REQUIRED");
        }
        catch (InvalidOperationException) when (version.Status != ContentVersionStatus.PendingHumanApproval)
        {
            return RequestContentChangesResult.InvalidState("CONTENT_VERSION_INVALID_STATE");
        }

        await changes.CommitAsync(cancellationToken);
        return RequestContentChangesResult.Changed(version.Version);
    }
}
