namespace ContentOS.Contracts.Internal;

public sealed record SnapshotByHashResponse(
    Guid SnapshotId,
    Guid SourceId,
    string ContentHash,
    string MediaType,
    long ByteLength,
    DateTimeOffset CapturedAt);
