namespace ContentOS.Application.Content.Topics;

public sealed record ApplyTopicLabelsResult
{
    private ApplyTopicLabelsResult(bool isSuccess, bool isDuplicate, string? errorCode)
    {
        IsSuccess = isSuccess;
        IsDuplicate = isDuplicate;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }

    public bool IsDuplicate { get; }

    public string? ErrorCode { get; }

    public static ApplyTopicLabelsResult Applied() => new(true, false, null);

    public static ApplyTopicLabelsResult Duplicate() => new(true, true, null);

    public static ApplyTopicLabelsResult NotFound() => new(false, false, "TOPIC_DISCOVERY_NOT_FOUND");

    public static ApplyTopicLabelsResult Invalid(string errorCode) => new(false, false, errorCode);
}
