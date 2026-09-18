namespace ContentOS.Application.Content.Generation;

public sealed record ContentGenerationBrief(
    Guid ContentUnitId,
    Guid ContentVersionId,
    Guid OperationId,
    string Title,
    string? Brief,
    string Format,
    string FormatMold,
    IReadOnlyList<string> CitationContentHashes,
    bool RequestReviewAfterGenerate);
