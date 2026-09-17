namespace ContentOS.Application.Learning.EvaluateCapstone;

public sealed record EvaluateCapstoneResult
{
    private EvaluateCapstoneResult(bool isSuccess, Guid? outcomeId, string? errorCode)
    {
        IsSuccess = isSuccess;
        OutcomeId = outcomeId;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }

    public Guid? OutcomeId { get; }

    public string? ErrorCode { get; }

    public static EvaluateCapstoneResult Evaluated(Guid outcomeId) =>
        new(true, outcomeId, null);

    public static EvaluateCapstoneResult NotFound() =>
        new(false, null, "CAPSTONE_NOT_FOUND");

    public static EvaluateCapstoneResult InvalidState() =>
        new(false, null, "CAPSTONE_INVALID_STATE");
}
