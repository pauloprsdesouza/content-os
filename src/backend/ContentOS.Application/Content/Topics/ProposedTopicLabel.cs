namespace ContentOS.Application.Content.Topics;

public sealed record ProposedTopicLabel(string Label, string? Rationale, IReadOnlyList<string> WorkIds);
