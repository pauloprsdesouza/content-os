using ContentOS.Domain.Knowledge;

namespace ContentOS.Application.Knowledge.Ports;

public sealed record ClaimReviewDetail(
    Guid Id,
    string Statement,
    ClaimStatus Status,
    decimal Confidence,
    long Version,
    string? RejectionReason,
    Guid? ReviewedByUserId,
    DateTimeOffset? ReviewedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ClaimEvidenceDetail> Evidence);
