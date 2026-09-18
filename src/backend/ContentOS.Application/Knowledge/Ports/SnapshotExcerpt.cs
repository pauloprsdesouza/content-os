namespace ContentOS.Application.Knowledge.Ports;

public sealed record SnapshotExcerpt(
    Guid SnapshotId,
    string ContentHash,
    string MediaType,
    string Text);
