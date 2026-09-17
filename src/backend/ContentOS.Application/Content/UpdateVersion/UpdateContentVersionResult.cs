namespace ContentOS.Application.Content.UpdateVersion;

public sealed record UpdateContentVersionResult
{
    private UpdateContentVersionResult(
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

    public static UpdateContentVersionResult Updated(long newVersion) =>
        new(true, newVersion, null, null);

    public static UpdateContentVersionResult NotFound() =>
        new(false, null, null, "CONTENT_VERSION_NOT_FOUND");

    public static UpdateContentVersionResult ConcurrencyConflict(long currentVersion) =>
        new(false, null, currentVersion, "CONTENT_VERSION_STALE");

    public static UpdateContentVersionResult InvalidState(string code) =>
        new(false, null, null, code);
}
