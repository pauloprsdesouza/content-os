namespace ContentOS.Contracts.Research;

public sealed record CreateResearchJobRequest(
    string Topic,
    string? ScopeNotes);
