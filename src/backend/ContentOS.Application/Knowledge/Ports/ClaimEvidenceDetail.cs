namespace ContentOS.Application.Knowledge.Ports;

public sealed record ClaimEvidenceDetail(
    Guid EvidenceId,
    Guid SnapshotId,
    string Locator,
    string ExtractionMethod,
    decimal Confidence);
