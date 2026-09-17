namespace ContentOS.Application.Content.Approve;

public sealed record ApproveContentVersionResult
{
    private ApproveContentVersionResult(
        bool isSuccess,
        long? newVersion,
        long? currentVersion,
        string? errorCode)
    {
        IsSuccess = isSuccess;
        NewVersion = newVersion;
        CurrentVersion = currentVersion;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }

    public long? NewVersion { get; }

    public long? CurrentVersion { get; }

    public string? ErrorCode { get; }

    public static ApproveContentVersionResult Approved(long newVersion) =>
        new(true, newVersion, null, null);

    public static ApproveContentVersionResult NotFound() =>
        new(false, null, null, "CONTENT_VERSION_NOT_FOUND");

    public static ApproveContentVersionResult ConcurrencyConflict(long currentVersion) =>
        new(false, null, currentVersion, "CONTENT_VERSION_STALE");

    public static ApproveContentVersionResult InvalidState(string code) =>
        new(false, null, null, code);
}
