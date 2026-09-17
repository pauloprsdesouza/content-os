namespace ContentOS.Contracts.Research;

public sealed record ResearchJobResponse(
    Guid Id,
    string Topic,
    string? ScopeNotes,
    string Status,
    long Version,
    int AttemptCount,
    string? FailureReason,
    Guid? OperationId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
