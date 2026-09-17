namespace ContentOS.Application.Learning.SubmitCapstone;

public sealed record SubmitCapstoneResult
{
    private SubmitCapstoneResult(bool isSuccess, Guid? capstoneId, string? errorCode)
    {
        IsSuccess = isSuccess;
        CapstoneId = capstoneId;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }

    public Guid? CapstoneId { get; }

    public string? ErrorCode { get; }

    public static SubmitCapstoneResult Submitted(Guid capstoneId) =>
        new(true, capstoneId, null);

    public static SubmitCapstoneResult NotFound() =>
        new(false, null, "CAPSTONE_NOT_FOUND");

    public static SubmitCapstoneResult InvalidState() =>
        new(false, null, "CAPSTONE_INVALID_STATE");
}
