namespace ContentOS.Contracts.Knowledge;

public sealed record CreateSnapshotRequest(
    string ContentBase64,
    string MediaType);
