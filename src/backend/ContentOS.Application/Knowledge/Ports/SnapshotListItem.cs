namespace ContentOS.Application.Knowledge.Ports;

public sealed record SnapshotListItem(
    Guid Id,
    Guid SourceId,
    string ContentHash,
    string MediaType,
    long ByteLength,
    DateTimeOffset CapturedAt);
