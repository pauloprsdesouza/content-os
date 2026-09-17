namespace ContentOS.Application.Publication.ConfirmPublication;

public sealed record ConfirmPublicationResult
{
    private ConfirmPublicationResult(
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

    public static ConfirmPublicationResult Confirmed(long newVersion) =>
        new(true, newVersion, null, null);

    public static ConfirmPublicationResult NotFound() =>
        new(false, null, null, "PUBLICATION_PACKAGE_NOT_FOUND");

    public static ConfirmPublicationResult ConcurrencyConflict(long currentVersion) =>
        new(false, null, currentVersion, "PUBLICATION_PACKAGE_STALE");

    public static ConfirmPublicationResult InvalidState(string code) =>
        new(false, null, null, code);
}
