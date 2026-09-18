using ContentOS.Application.Content.CreateUnit;
using ContentOS.Application.Content.Ports;
using ContentOS.Application.Knowledge.Capture;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Content;

namespace ContentOS.Application.Content.Topics;

public sealed class SelectDiscoveredTopicsHandler(
    ITopicDiscoveryRepository discoveries,
    CaptureSourceSnapshotHandler captures,
    CreateContentUnitHandler units,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<SelectDiscoveredTopicsResult> HandleAsync(
        SelectDiscoveredTopicsCommand command,
        CancellationToken cancellationToken = default)
    {
        var discovery = await discoveries.GetByIdAsync(command.DiscoveryId, cancellationToken);
        if (discovery is null || discovery.OwnerUserId != command.OwnerUserId)
        {
            return SelectDiscoveredTopicsResult.Invalid("TOPIC_DISCOVERY_NOT_FOUND");
        }

        if (discovery.Status != TopicDiscoveryStatus.AwaitingSelection)
        {
            return SelectDiscoveredTopicsResult.Invalid("TOPIC_SELECTION_CLOSED");
        }

        var selectedIds = command.ProposalIds.Where(id => id != Guid.Empty).Distinct().ToArray();
        if (selectedIds.Length == 0)
        {
            return SelectDiscoveredTopicsResult.Invalid("TOPIC_SELECTION_REQUIRED");
        }

        var proposals = await discoveries.ListProposalsAsync(discovery.Id, cancellationToken);
        var chosen = proposals.Where(proposal => selectedIds.Contains(proposal.Id)).ToArray();
        if (chosen.Length != selectedIds.Length)
        {
            return SelectDiscoveredTopicsResult.Invalid("TOPIC_SELECTION_UNKNOWN");
        }

        var works = await discoveries.ListWorksAsync(discovery.Id, cancellationToken);
        var hashes = new List<string>();
        foreach (var proposal in chosen)
        {
            var related = works
                .Where(work => proposal.ListWorkIds().Contains(work.WorkId, StringComparer.Ordinal))
                .ToArray();
            if (related.Length == 0)
            {
                return SelectDiscoveredTopicsResult.Invalid("TOPIC_WORK_MISSING");
            }

            var captured = await captures.HandleAsync(
                new CaptureSourceSnapshotCommand(
                    SourceCaptureMode.Text,
                    null,
                    proposal.Label,
                    null,
                    TopicEvidenceText.Build(proposal.Label, proposal.Rationale, related),
                    null,
                    null,
                    null),
                cancellationToken);
            if (!captured.IsSuccess || string.IsNullOrWhiteSpace(captured.ContentHash))
            {
                return SelectDiscoveredTopicsResult.Invalid(captured.ErrorCode ?? "CAPTURE_EMPTY");
            }

            hashes.Add(captured.ContentHash);
            proposal.Select();
        }

        var title = string.Join(" · ", chosen.Select(proposal => proposal.Label));
        if (title.Length > 500)
        {
            title = title[..500];
        }

        var brief = string.Join("\n", chosen.Select(proposal => proposal.Rationale).Where(text => !string.IsNullOrWhiteSpace(text)));
        if (brief.Length > 4000)
        {
            brief = brief[..4000];
        }

        var created = await units.HandleAsync(
            new CreateContentUnitCommand(
                title,
                string.IsNullOrWhiteSpace(brief) ? null : brief,
                true,
                command.OwnerUserId,
                discovery.Format.Code,
                hashes,
                discovery.Id,
                command.ProductId),
            cancellationToken);
        if (!created.IsSuccess || created.ContentUnitId is null || created.ContentVersionId is null)
        {
            return SelectDiscoveredTopicsResult.Invalid(created.ErrorCode ?? "CONTENT_CREATE_FAILED");
        }

        discovery.MarkWriting(created.ContentUnitId.Value, clock.GetUtcNow());
        await changes.CommitAsync(cancellationToken);
        return SelectDiscoveredTopicsResult.Created(
            created.ContentUnitId.Value,
            created.ContentVersionId.Value,
            created.OperationId);
    }
}
