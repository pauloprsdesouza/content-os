namespace ContentOS.Application.Research.ApplyResult;

public sealed record ApplyResearchResultResult
{
    private ApplyResearchResultResult(bool isSuccess, string? errorCode, bool alreadyApplied)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        AlreadyApplied = alreadyApplied;
    }

    public bool IsSuccess { get; }

    public string? ErrorCode { get; }

    public bool AlreadyApplied { get; }

    public static ApplyResearchResultResult Applied() => new(true, null, false);

    public static ApplyResearchResultResult Duplicate() => new(true, null, true);

    public static ApplyResearchResultResult NotFound() =>
        new(false, "RESEARCH_JOB_NOT_FOUND", false);

    public static ApplyResearchResultResult Invalid(string code) =>
        new(false, code, false);
}
