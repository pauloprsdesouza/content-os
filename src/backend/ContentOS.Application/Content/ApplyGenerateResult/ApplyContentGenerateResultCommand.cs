namespace ContentOS.Application.Content.ApplyGenerateResult;

public sealed record ApplyContentGenerateResultCommand(
    Guid ContentVersionId,
    Guid OperationId,
    string IdempotencyKey,
    bool Succeeded,
    string? ErrorMessage,
    string? BodyMarkdown,
    bool RequestReviewAfterGenerate);
