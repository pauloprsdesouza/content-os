using ContentOS.Domain.Operations;

namespace ContentOS.Application.Operations.Get;

public sealed record GetOperationResult
{
    private GetOperationResult(bool isSuccess, Operation? operation, string? errorCode)
    {
        IsSuccess = isSuccess;
        Operation = operation;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }

    public Operation? Operation { get; }

    public string? ErrorCode { get; }

    public static GetOperationResult Found(Operation operation) => new(true, operation, null);

    public static GetOperationResult NotFound() =>
        new(false, null, "OPERATION_NOT_FOUND");
}
