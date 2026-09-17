namespace ContentOS.Application.Research.Create;

public sealed record CreateResearchJobResult
{
    private CreateResearchJobResult(
        bool isSuccess,
        Guid? researchJobId,
        Guid? operationId,
        string? errorCode)
    {
        IsSuccess = isSuccess;
        ResearchJobId = researchJobId;
        OperationId = operationId;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }

    public Guid? ResearchJobId { get; }

    public Guid? OperationId { get; }

    public string? ErrorCode { get; }

    public static CreateResearchJobResult Created(Guid researchJobId, Guid operationId) =>
        new(true, researchJobId, operationId, null);

    public static CreateResearchJobResult Invalid(string errorCode) =>
        new(false, null, null, errorCode);
}
