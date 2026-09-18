namespace ContentOS.Application.Content.Series;

public sealed record CreateEditorialSeriesResult
{
    private CreateEditorialSeriesResult(bool isSuccess, Guid? seriesId, string? errorCode)
    {
        IsSuccess = isSuccess;
        SeriesId = seriesId;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }

    public Guid? SeriesId { get; }

    public string? ErrorCode { get; }

    public static CreateEditorialSeriesResult Created(Guid seriesId) => new(true, seriesId, null);

    public static CreateEditorialSeriesResult Invalid(string errorCode) => new(false, null, errorCode);
}
