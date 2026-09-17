namespace ContentOS.Application.Catalog.ReplaceCurriculum;

public sealed record ReplaceCurriculumResult
{
    private ReplaceCurriculumResult(
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

    public static ReplaceCurriculumResult Replaced(long newVersion) =>
        new(true, newVersion, null, null);

    public static ReplaceCurriculumResult NotFound() =>
        new(false, null, null, "EDITION_NOT_FOUND");

    public static ReplaceCurriculumResult ConcurrencyConflict(long currentVersion) =>
        new(false, null, currentVersion, "EDITION_STALE");

    public static ReplaceCurriculumResult InvalidCurriculum(string code) =>
        new(false, null, null, code);
}
