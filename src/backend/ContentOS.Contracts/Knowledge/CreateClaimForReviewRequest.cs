namespace ContentOS.Contracts.Knowledge;

public sealed record CreateClaimForReviewRequest(
    string Statement,
    decimal Confidence,
    Guid SnapshotId,
    string EvidenceLocator,
    string ExtractionMethod);
