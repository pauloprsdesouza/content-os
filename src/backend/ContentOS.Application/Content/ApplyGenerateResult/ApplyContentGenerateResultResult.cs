namespace ContentOS.Application.Content.ApplyGenerateResult;

public sealed record ApplyContentGenerateResultResult
{
    private ApplyContentGenerateResultResult(bool isSuccess, string? errorCode, bool alreadyApplied)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        AlreadyApplied = alreadyApplied;
    }

    public bool IsSuccess { get; }

    public string? ErrorCode { get; }

    public bool AlreadyApplied { get; }

    public static ApplyContentGenerateResultResult Applied() => new(true, null, false);

    public static ApplyContentGenerateResultResult Duplicate() => new(true, null, true);

    public static ApplyContentGenerateResultResult NotFound() =>
        new(false, "CONTENT_VERSION_NOT_FOUND", false);

    public static ApplyContentGenerateResultResult Invalid(string code) =>
        new(false, code, false);
}
