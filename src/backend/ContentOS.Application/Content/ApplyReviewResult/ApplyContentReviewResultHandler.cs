using ContentOS.Application.Content.Ports;
using ContentOS.Application.Operations.Ports;
using ContentOS.Application.Operations.Progress;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Content;
using ContentOS.Domain.Operations;

namespace ContentOS.Application.Content.ApplyReviewResult;

public sealed class ApplyContentReviewResultHandler(
    IContentVersionRepository versions,
    IOperationRepository operations,
    TimeProvider clock,
    IChangeCommitter changes,
    IOperationProgress progress)
{
    public async Task<ApplyContentReviewResultResult> HandleAsync(
        ApplyContentReviewResultCommand command,
        CancellationToken cancellationToken = default)
    {
        var version = await versions.GetByIdAsync(command.ContentVersionId, cancellationToken);
        if (version is null)
        {
            return ApplyContentReviewResultResult.NotFound();
        }

        var operation = await operations.GetByIdAsync(command.OperationId, cancellationToken);
        if (operation is null)
        {
            return ApplyContentReviewResultResult.Invalid("OPERATION_NOT_FOUND");
        }

        if (operation.Status is OperationStatus.Succeeded or OperationStatus.Failed)
        {
            return ApplyContentReviewResultResult.Duplicate();
        }

        var now = clock.GetUtcNow();

        if (version.Status is ContentVersionStatus.PendingHumanApproval
            or ContentVersionStatus.Approved
            or ContentVersionStatus.AgentReviewed)
        {
            return ApplyContentReviewResultResult.Duplicate();
        }

        if (!command.Succeeded)
        {
            operation.MarkFailed(command.ErrorMessage ?? "Content review failed.", now);
            await changes.CommitAsync(cancellationToken);
            OperationProgressNotifications.Notify(progress, operation);
            return ApplyContentReviewResultResult.Applied();
        }

        if (version.Status != ContentVersionStatus.ReviewQueued)
        {
            return ApplyContentReviewResultResult.Invalid("CONTENT_REVIEW_INVALID_STATE");
        }

        try
        {
            version.MarkAgentReviewed(command.AgentReviewNotes, now);
            version.RequestHumanApproval(now);
            operation.MarkSucceeded(now);
        }
        catch (InvalidOperationException)
        {
            return ApplyContentReviewResultResult.Invalid("CONTENT_REVIEW_INVALID_STATE");
        }

        await changes.CommitAsync(cancellationToken);
        OperationProgressNotifications.Notify(progress, operation);
        return ApplyContentReviewResultResult.Applied();
    }
}
