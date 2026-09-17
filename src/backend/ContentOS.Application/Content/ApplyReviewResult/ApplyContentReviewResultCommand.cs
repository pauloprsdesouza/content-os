namespace ContentOS.Application.Content.ApplyReviewResult;

public sealed record ApplyContentReviewResultCommand(
    Guid ContentVersionId,
    Guid OperationId,
    string IdempotencyKey,
    bool Succeeded,
    string? ErrorMessage,
    string? AgentReviewNotes);
