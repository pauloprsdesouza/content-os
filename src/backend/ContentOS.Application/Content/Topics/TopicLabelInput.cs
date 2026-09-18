namespace ContentOS.Application.Content.Topics;

public sealed record TopicLabelInput(string Label, string? Rationale, IReadOnlyList<string> WorkIds);
