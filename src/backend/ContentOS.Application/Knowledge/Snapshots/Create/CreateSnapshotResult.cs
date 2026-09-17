namespace ContentOS.Application.Knowledge.Snapshots.Create;

public sealed record CreateSnapshotResult
{
    private CreateSnapshotResult(
        bool isSuccess,
        Guid? snapshotId,
        string? contentHash,
        string? errorCode)
    {
        IsSuccess = isSuccess;
        SnapshotId = snapshotId;
        ContentHash = contentHash;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }

    public Guid? SnapshotId { get; }

    public string? ContentHash { get; }

    public string? ErrorCode { get; }

    public static CreateSnapshotResult Created(Guid snapshotId, string contentHash) =>
        new(true, snapshotId, contentHash, null);

    public static CreateSnapshotResult SourceNotFound() =>
        new(false, null, null, "SOURCE_NOT_FOUND");
}
