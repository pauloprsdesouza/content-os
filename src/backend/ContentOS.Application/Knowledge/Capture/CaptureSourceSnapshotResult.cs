namespace ContentOS.Application.Knowledge.Capture;

public sealed record CaptureSourceSnapshotResult
{
    private CaptureSourceSnapshotResult(
        bool isSuccess,
        Guid? sourceId,
        Guid? snapshotId,
        string? contentHash,
        bool sourceCreated,
        string? errorCode)
    {
        IsSuccess = isSuccess;
        SourceId = sourceId;
        SnapshotId = snapshotId;
        ContentHash = contentHash;
        SourceCreated = sourceCreated;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }

    public Guid? SourceId { get; }

    public Guid? SnapshotId { get; }

    public string? ContentHash { get; }

    public bool SourceCreated { get; }

    public string? ErrorCode { get; }

    public static CaptureSourceSnapshotResult Created(
        Guid sourceId,
        Guid snapshotId,
        string contentHash,
        bool sourceCreated) =>
        new(true, sourceId, snapshotId, contentHash, sourceCreated, null);

    public static CaptureSourceSnapshotResult Invalid(string errorCode) =>
        new(false, null, null, null, false, errorCode);
}
