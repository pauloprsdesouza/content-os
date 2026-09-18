using ContentOS.Application.Persistence;
using ContentOS.Application.Research.Ports;
using ContentOS.Domain.Research;

namespace ContentOS.Application.Research.DiscardJob;

public sealed class DiscardResearchJobHandler(
    IResearchJobRepository jobs,
    IChangeCommitter changes)
{
    public async Task<DiscardOutcome> HandleAsync(
        DiscardResearchJobCommand command,
        CancellationToken cancellationToken = default)
    {
        var job = await jobs.GetByIdAsync(command.ResearchJobId, cancellationToken);
        if (job is null)
        {
            return DiscardOutcome.Missing("RESEARCH_NOT_FOUND");
        }

        if (job.Status is ResearchJobStatus.AwaitingKnowledgeReview or ResearchJobStatus.Completed)
        {
            return DiscardOutcome.Blocked("RESEARCH_IN_REVIEW");
        }

        await jobs.DeleteGraphAsync(command.ResearchJobId, cancellationToken);
        await changes.CommitAsync(cancellationToken);
        return DiscardOutcome.Ok();
    }
}
