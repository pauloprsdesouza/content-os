namespace ContentOS.Contracts.Knowledge;

public sealed record ClaimEvidenceResponse(
    Guid EvidenceId,
    Guid SnapshotId,
    string Locator,
    string ExtractionMethod,
    decimal Confidence);
