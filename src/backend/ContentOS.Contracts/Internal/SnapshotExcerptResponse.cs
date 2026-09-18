namespace ContentOS.Contracts.Internal;

public sealed record SnapshotExcerptResponse(
    Guid SnapshotId,
    string ContentHash,
    string MediaType,
    string Text);
