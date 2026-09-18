using ContentOS.Application.Knowledge.Claims.Promote;
using ContentOS.Application.Operations.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Application.Research.Ports;
using ContentOS.Domain.Operations;
using ContentOS.Domain.Research;
using ContentOS.SharedKernel;

namespace ContentOS.Application.Research.ApplyResult;

public sealed class ApplyResearchResultHandler(
    IResearchJobRepository jobs,
    IOperationRepository operations,
    IIdGenerator ids,
    TimeProvider clock,
    IChangeCommitter changes,
    PromoteResearchFindingsHandler promoter)
{
    public async Task<ApplyResearchResultResult> HandleAsync(
        ApplyResearchResultCommand command,
        CancellationToken cancellationToken = default)
    {
        var job = await jobs.GetByIdAsync(command.ResearchJobId, cancellationToken);
        if (job is null)
        {
            return ApplyResearchResultResult.NotFound();
        }

        var operation = await operations.GetByIdAsync(command.OperationId, cancellationToken);
        if (operation is null)
        {
            return ApplyResearchResultResult.Invalid("OPERATION_NOT_FOUND");
        }

        if (operation.Status is OperationStatus.Succeeded or OperationStatus.Failed)
        {
            return ApplyResearchResultResult.Duplicate();
        }

        var now = clock.GetUtcNow();

        if (job.Status is ResearchJobStatus.AwaitingKnowledgeReview
            or ResearchJobStatus.Completed
            or ResearchJobStatus.Failed)
        {
            return ApplyResearchResultResult.Duplicate();
        }

        if (job.Status is ResearchJobStatus.Queued or ResearchJobStatus.RetryScheduled)
        {
            job.MarkRunning(now);
        }

        if (!command.Succeeded)
        {
            job.MarkFailed(command.ErrorMessage ?? "Research job failed.", now);
            operation.MarkFailed(command.ErrorMessage ?? "Research job failed.", now);
            await changes.CommitAsync(cancellationToken);
            return ApplyResearchResultResult.Applied();
        }

        if (job.Status != ResearchJobStatus.Running)
        {
            return ApplyResearchResultResult.Invalid("RESEARCH_INVALID_STATE");
        }

        var findings = command.Findings
            .Select(finding =>
            {
                var findingId = finding.FindingId ?? ids.NewId();
                return (Input: finding, Entity: ResearchFinding.Create(
                    findingId,
                    job.Id,
                    finding.Statement,
                    finding.Confidence,
                    now));
            })
            .ToList();

        await promoter.HandleAsync(
            new PromoteResearchFindingsCommand(
                job.Id,
                findings.Select(item => new PromoteResearchFindingItem(
                    item.Entity.Id,
                    item.Input.Statement,
                    item.Input.Confidence,
                    item.Input.SourceSnapshotId,
                    item.Input.Locator,
                    item.Input.ExtractionMethod)).ToList()),
            cancellationToken);

        job.MarkAwaitingKnowledgeReview(findings.Select(item => item.Entity).ToList(), now);
        operation.MarkSucceeded(now);
        await changes.CommitAsync(cancellationToken);
        return ApplyResearchResultResult.Applied();
    }
}
