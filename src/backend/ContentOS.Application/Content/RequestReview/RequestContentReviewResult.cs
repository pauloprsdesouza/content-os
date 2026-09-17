namespace ContentOS.Application.Content.RequestReview;

public sealed record RequestContentReviewResult
{
    private RequestContentReviewResult(
        bool isSuccess,
        Guid? operationId,
        string? errorCode)
    {
        IsSuccess = isSuccess;
        OperationId = operationId;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }

    public Guid? OperationId { get; }

    public string? ErrorCode { get; }

    public static RequestContentReviewResult Accepted(Guid operationId) =>
        new(true, operationId, null);

    public static RequestContentReviewResult NotFound() =>
        new(false, null, "CONTENT_VERSION_NOT_FOUND");

    public static RequestContentReviewResult Invalid(string code) =>
        new(false, null, code);
}
