using ContentOS.SharedKernel;

namespace ContentOS.Application.Knowledge.Claims.CreateForReview;

public sealed record CreateClaimForReviewCommand(
    string Statement,
    decimal Confidence,
    Guid SnapshotId,
    string EvidenceLocator,
    string ExtractionMethod,
    IdempotencyKey IdempotencyKey);
