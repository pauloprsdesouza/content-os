using ContentOS.Application.Content.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Content;

namespace ContentOS.Application.Content.Approve;

public sealed class ApproveContentVersionHandler(
    IContentVersionRepository versions,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<ApproveContentVersionResult> HandleAsync(
        ApproveContentVersionCommand command,
        CancellationToken cancellationToken = default)
    {
        var version = await versions.GetByIdAsync(command.ContentVersionId, cancellationToken);
        if (version is null)
        {
            return ApproveContentVersionResult.NotFound();
        }

        if (version.Version != command.ExpectedVersion)
        {
            return ApproveContentVersionResult.ConcurrencyConflict(version.Version);
        }

        try
        {
            version.Approve(command.ActorUserId, command.ExpectedVersion, clock.GetUtcNow());
        }
        catch (InvalidOperationException) when (version.Status != ContentVersionStatus.PendingHumanApproval)
        {
            return ApproveContentVersionResult.InvalidState("CONTENT_VERSION_INVALID_STATE");
        }

        await changes.CommitAsync(cancellationToken);
        return ApproveContentVersionResult.Approved(version.Version);
    }
}
