namespace ContentOS.Application.Content.CreateUnit;

public sealed record CreateContentUnitResult
{
    private CreateContentUnitResult(
        bool isSuccess,
        Guid? contentUnitId,
        Guid? contentVersionId,
        Guid? operationId,
        string? errorCode)
    {
        IsSuccess = isSuccess;
        ContentUnitId = contentUnitId;
        ContentVersionId = contentVersionId;
        OperationId = operationId;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }

    public Guid? ContentUnitId { get; }

    public Guid? ContentVersionId { get; }

    public Guid? OperationId { get; }

    public string? ErrorCode { get; }

    public static CreateContentUnitResult Created(
        Guid contentUnitId,
        Guid contentVersionId,
        Guid? operationId) =>
        new(true, contentUnitId, contentVersionId, operationId, null);

    public static CreateContentUnitResult Invalid(string errorCode) =>
        new(false, null, null, null, errorCode);
}
