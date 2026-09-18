namespace ContentOS.Contracts.Content;

public sealed record TopicProposalResponse(
    Guid Id,
    string Label,
    string? Rationale,
    IReadOnlyList<string> WorkIds,
    bool IsSelected);
