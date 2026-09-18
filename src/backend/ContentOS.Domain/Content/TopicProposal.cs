namespace ContentOS.Domain.Content;

public sealed class TopicProposal
{
    private TopicProposal(
        Guid id,
        Guid discoveryId,
        string label,
        string? rationale,
        string workIds)
    {
        Id = id;
        DiscoveryId = discoveryId;
        Label = label;
        Rationale = rationale;
        WorkIds = workIds;
    }

    private TopicProposal()
    {
        Label = null!;
        WorkIds = null!;
    }

    public Guid Id { get; private set; }

    public Guid DiscoveryId { get; private set; }

    public string Label { get; private set; }

    public string? Rationale { get; private set; }

    public string WorkIds { get; private set; }

    public bool IsSelected { get; private set; }

    public IReadOnlyList<string> ListWorkIds() =>
        WorkIds.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public static TopicProposal Create(
        Guid id,
        Guid discoveryId,
        string label,
        string? rationale,
        IReadOnlyList<string> workIds)
    {
        if (id == Guid.Empty || discoveryId == Guid.Empty)
        {
            throw new ArgumentException("Proposal and discovery ids are required.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        if (workIds is null || workIds.Count == 0 || workIds.All(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("A topic requires at least one work id.", nameof(workIds));
        }

        var normalized = workIds
            .Where(workId => !string.IsNullOrWhiteSpace(workId))
            .Select(workId => workId.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (normalized.Length == 0 || normalized.Any(workId => workId.Contains("://", StringComparison.Ordinal)))
        {
            throw new ArgumentException("A topic requires at least one work id.", nameof(workIds));
        }

        return new TopicProposal(
            id,
            discoveryId,
            label.Trim(),
            string.IsNullOrWhiteSpace(rationale) ? null : rationale.Trim(),
            string.Join('\n', normalized));
    }

    public void Select() => IsSelected = true;
}
