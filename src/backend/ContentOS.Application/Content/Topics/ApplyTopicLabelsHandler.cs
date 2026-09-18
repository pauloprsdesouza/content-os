using ContentOS.Application.Content.Ports;
using ContentOS.Application.Operations.Ports;
using ContentOS.Application.Operations.Progress;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Content;
using ContentOS.Domain.Operations;
using ContentOS.SharedKernel;

namespace ContentOS.Application.Content.Topics;

public sealed class ApplyTopicLabelsHandler(
    ITopicDiscoveryRepository discoveries,
    IOperationRepository operations,
    IIdGenerator ids,
    TimeProvider clock,
    IChangeCommitter changes,
    IOperationProgress progress)
{
    public async Task<ApplyTopicLabelsResult> HandleAsync(
        ApplyTopicLabelsCommand command,
        CancellationToken cancellationToken = default)
    {
        var discovery = await discoveries.GetByIdAsync(command.DiscoveryId, cancellationToken);
        if (discovery is null)
        {
            return ApplyTopicLabelsResult.NotFound();
        }

        var operation = await operations.GetByIdAsync(command.OperationId, cancellationToken);
        if (operation is null)
        {
            return ApplyTopicLabelsResult.Invalid("OPERATION_NOT_FOUND");
        }

        if (operation.Status is OperationStatus.Succeeded or OperationStatus.Failed)
        {
            return ApplyTopicLabelsResult.Duplicate();
        }

        var now = clock.GetUtcNow();
        if (!command.Succeeded)
        {
            discovery.MarkAwaitingSelection(now);
            operation.MarkFailed(command.ErrorMessage ?? "Topic labeling failed.", now);
            await changes.CommitAsync(cancellationToken);
            OperationProgressNotifications.Notify(progress, operation);
            return ApplyTopicLabelsResult.Applied();
        }

        var known = (await discoveries.ListWorksAsync(discovery.Id, cancellationToken))
            .Select(work => work.WorkId)
            .ToHashSet(StringComparer.Ordinal);
        var accepted = TopicLabelGate.Accept(
            known,
            command.Topics
                .Select(topic => new ProposedTopicLabel(topic.Label, topic.Rationale, topic.WorkIds))
                .ToArray());

        if (accepted.Count > 0)
        {
            discoveries.AddProposals(accepted.Select(topic =>
                TopicProposal.Create(
                    ids.NewId(),
                    discovery.Id,
                    topic.Label,
                    topic.Rationale,
                    topic.WorkIds)));
        }

        discovery.MarkAwaitingSelection(now);
        operation.MarkSucceeded(now);
        await changes.CommitAsync(cancellationToken);
        OperationProgressNotifications.Notify(progress, operation);
        return ApplyTopicLabelsResult.Applied();
    }
}
