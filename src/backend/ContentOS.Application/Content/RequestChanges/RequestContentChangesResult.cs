namespace ContentOS.Application.Content.RequestChanges;

public sealed record RequestContentChangesResult
{
    private RequestContentChangesResult(
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

    public static RequestContentChangesResult Changed(long newVersion) =>
        new(true, newVersion, null, null);

    public static RequestContentChangesResult NotFound() =>
        new(false, null, null, "CONTENT_VERSION_NOT_FOUND");

    public static RequestContentChangesResult ConcurrencyConflict(long currentVersion) =>
        new(false, null, currentVersion, "CONTENT_VERSION_STALE");

    public static RequestContentChangesResult InvalidState(string code) =>
        new(false, null, null, code);
}
