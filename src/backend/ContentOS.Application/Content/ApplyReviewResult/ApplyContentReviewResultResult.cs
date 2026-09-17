namespace ContentOS.Application.Content.ApplyReviewResult;

public sealed record ApplyContentReviewResultResult
{
    private ApplyContentReviewResultResult(bool isSuccess, string? errorCode, bool alreadyApplied)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        AlreadyApplied = alreadyApplied;
    }

    public bool IsSuccess { get; }

    public string? ErrorCode { get; }

    public bool AlreadyApplied { get; }

    public static ApplyContentReviewResultResult Applied() => new(true, null, false);

    public static ApplyContentReviewResultResult Duplicate() => new(true, null, true);

    public static ApplyContentReviewResultResult NotFound() =>
        new(false, "CONTENT_VERSION_NOT_FOUND", false);

    public static ApplyContentReviewResultResult Invalid(string code) =>
        new(false, code, false);
}
