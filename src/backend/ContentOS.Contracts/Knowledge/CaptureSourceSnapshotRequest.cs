namespace ContentOS.Contracts.Knowledge;

public sealed record CaptureSourceSnapshotRequest(
    string Mode,
    string? DisplayName,
    string? Location,
    string? Text);
