namespace ContentOS.Contracts.Knowledge;

public sealed record SnapshotResponse(
    Guid Id,
    Guid SourceId,
    string ContentHash,
    string MediaType,
    long ByteLength,
    DateTimeOffset CapturedAt);
