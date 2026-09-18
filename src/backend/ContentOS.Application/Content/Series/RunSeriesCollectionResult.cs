namespace ContentOS.Application.Content.Series;

public sealed record RunSeriesCollectionResult
{
    private RunSeriesCollectionResult(bool isSuccess, Guid? discoveryId, string? errorCode)
    {
        IsSuccess = isSuccess;
        DiscoveryId = discoveryId;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }

    public Guid? DiscoveryId { get; }

    public string? ErrorCode { get; }

    public static RunSeriesCollectionResult Completed(Guid discoveryId) => new(true, discoveryId, null);

    public static RunSeriesCollectionResult Invalid(string errorCode) => new(false, null, errorCode);
}
