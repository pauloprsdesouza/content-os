namespace ContentOS.Application.Knowledge.Claims.Approve;

public sealed record ApproveClaimResult
{
    private ApproveClaimResult(
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

    public static ApproveClaimResult Approved(long newVersion) =>
        new(true, newVersion, null, null);

    public static ApproveClaimResult NotFound() =>
        new(false, null, null, "CLAIM_NOT_FOUND");

    public static ApproveClaimResult ConcurrencyConflict(long currentVersion) =>
        new(false, null, currentVersion, "CLAIM_STALE");

    public static ApproveClaimResult InvalidState(string errorCode) =>
        new(false, null, null, errorCode);
}
