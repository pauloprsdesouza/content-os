namespace ContentOS.Contracts.Content;

public sealed record ContentVersionResponse(
    Guid Id,
    Guid ContentUnitId,
    int Revision,
    string? BodyMarkdown,
    string Status,
    long Version,
    string? AgentReviewNotes,
    string? ChangeRequestNotes,
    Guid? ReviewedByUserId,
    DateTimeOffset? ReviewedAt,
    Guid? OperationId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
