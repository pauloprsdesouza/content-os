namespace ContentOS.Application.Research.ApplyResult;

public sealed record ApplyResearchResultCommand(
    Guid ResearchJobId,
    Guid OperationId,
    string IdempotencyKey,
    bool Succeeded,
    string? ErrorMessage,
    IReadOnlyList<ResearchFindingInput> Findings);
