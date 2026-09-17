namespace ContentOS.Application.Knowledge.Claims.Reject;

public sealed record RejectClaimResult
{
    private RejectClaimResult(
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

    public static RejectClaimResult Rejected(long newVersion) =>
        new(true, newVersion, null, null);

    public static RejectClaimResult NotFound() =>
        new(false, null, null, "CLAIM_NOT_FOUND");

    public static RejectClaimResult ConcurrencyConflict(long currentVersion) =>
        new(false, null, currentVersion, "CLAIM_STALE");

    public static RejectClaimResult InvalidState(string errorCode) =>
        new(false, null, null, errorCode);
}
