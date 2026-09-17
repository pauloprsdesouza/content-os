using ContentOS.Application.Content.Ports;
using ContentOS.Application.Operations.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Content;
using ContentOS.Domain.Operations;

namespace ContentOS.Application.Content.ApplyGenerateResult;

public sealed class ApplyContentGenerateResultHandler(
    IContentVersionRepository versions,
    IOperationRepository operations,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<ApplyContentGenerateResultResult> HandleAsync(
        ApplyContentGenerateResultCommand command,
        CancellationToken cancellationToken = default)
    {
        var version = await versions.GetByIdAsync(command.ContentVersionId, cancellationToken);
        if (version is null)
        {
            return ApplyContentGenerateResultResult.NotFound();
        }

        var operation = await operations.GetByIdAsync(command.OperationId, cancellationToken);
        if (operation is null)
        {
            return ApplyContentGenerateResultResult.Invalid("OPERATION_NOT_FOUND");
        }

        if (operation.Status is OperationStatus.Succeeded or OperationStatus.Failed)
        {
            return ApplyContentGenerateResultResult.Duplicate();
        }

        var now = clock.GetUtcNow();

        if (version.Status is ContentVersionStatus.Generated
            or ContentVersionStatus.PendingHumanApproval
            or ContentVersionStatus.Approved)
        {
            return ApplyContentGenerateResultResult.Duplicate();
        }

        if (!command.Succeeded)
        {
            operation.MarkFailed(command.ErrorMessage ?? "Content generation failed.", now);
            await changes.CommitAsync(cancellationToken);
            return ApplyContentGenerateResultResult.Applied();
        }

        if (version.Status != ContentVersionStatus.GenerationQueued)
        {
            return ApplyContentGenerateResultResult.Invalid("CONTENT_GENERATE_INVALID_STATE");
        }

        try
        {
            version.MarkGenerated(command.BodyMarkdown ?? string.Empty, now);
            if (command.RequestReviewAfterGenerate)
            {
                version.RequestHumanApproval(now);
            }

            operation.MarkSucceeded(now);
        }
        catch (InvalidOperationException)
        {
            return ApplyContentGenerateResultResult.Invalid("CONTENT_GENERATE_INVALID_STATE");
        }
        catch (ArgumentException)
        {
            return ApplyContentGenerateResultResult.Invalid("CONTENT_BODY_REQUIRED");
        }

        await changes.CommitAsync(cancellationToken);
        return ApplyContentGenerateResultResult.Applied();
    }
}
