namespace ContentOS.Application.Knowledge.Ports;

public sealed record SnapshotByHashDetail(
    Guid SnapshotId,
    Guid SourceId,
    string ContentHash,
    string MediaType,
    long ByteLength,
    DateTimeOffset CapturedAt);
