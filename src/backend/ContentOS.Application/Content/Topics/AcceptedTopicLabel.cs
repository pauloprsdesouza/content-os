namespace ContentOS.Application.Content.Topics;

public sealed record AcceptedTopicLabel(string Label, string? Rationale, IReadOnlyList<string> WorkIds);
