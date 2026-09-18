namespace ContentOS.Contracts.Knowledge;

public sealed record CaptureSourceSnapshotResponse(
    Guid SourceId,
    Guid SnapshotId,
    string ContentHash,
    bool SourceCreated);
