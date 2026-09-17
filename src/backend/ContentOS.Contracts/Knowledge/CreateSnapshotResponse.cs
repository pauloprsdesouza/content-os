namespace ContentOS.Contracts.Knowledge;

public sealed record CreateSnapshotResponse(
    Guid Id,
    string ContentHash);
