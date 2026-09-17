namespace ContentOS.Contracts.Knowledge;

public sealed record ClaimResponse(
    Guid Id,
    string Statement,
    string Status,
    decimal Confidence,
    long Version,
    string? RejectionReason,
    Guid? ReviewedByUserId,
    DateTimeOffset? ReviewedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ClaimEvidenceResponse> Evidence);
