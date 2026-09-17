namespace ContentOS.Contracts.Common;

public sealed record OperationResponse(
    Guid Id,
    string Kind,
    string SubjectType,
    Guid SubjectId,
    string Status,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
