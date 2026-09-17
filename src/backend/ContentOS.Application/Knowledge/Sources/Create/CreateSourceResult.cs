namespace ContentOS.Application.Knowledge.Sources.Create;

public sealed record CreateSourceResult
{
    private CreateSourceResult(bool isSuccess, Guid? sourceId, string? errorCode)
    {
        IsSuccess = isSuccess;
        SourceId = sourceId;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }

    public Guid? SourceId { get; }

    public string? ErrorCode { get; }

    public static CreateSourceResult Created(Guid sourceId) =>
        new(true, sourceId, null);

    public static CreateSourceResult Duplicate() =>
        new(false, null, "SOURCE_DUPLICATE_URI");

    public static CreateSourceResult InvalidUri() =>
        new(false, null, "SOURCE_INVALID_URI");
}
