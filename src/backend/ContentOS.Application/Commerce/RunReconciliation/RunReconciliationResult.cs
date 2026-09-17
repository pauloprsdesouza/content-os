namespace ContentOS.Application.Commerce.RunReconciliation;

public sealed record RunReconciliationResult
{
    private RunReconciliationResult(
        bool isSuccess,
        Guid? runId,
        int processedCount,
        int confirmedCount,
        int ignoredCount,
        string? errorCode,
        string? detail)
    {
        IsSuccess = isSuccess;
        RunId = runId;
        ProcessedCount = processedCount;
        ConfirmedCount = confirmedCount;
        IgnoredCount = ignoredCount;
        ErrorCode = errorCode;
        Detail = detail;
    }

    public bool IsSuccess { get; }

    public Guid? RunId { get; }

    public int ProcessedCount { get; }

    public int ConfirmedCount { get; }

    public int IgnoredCount { get; }

    public string? ErrorCode { get; }

    public string? Detail { get; }

    public static RunReconciliationResult Completed(
        Guid runId,
        int processedCount,
        int confirmedCount,
        int ignoredCount) =>
        new(true, runId, processedCount, confirmedCount, ignoredCount, null, null);

    public static RunReconciliationResult Failed(string errorCode, string detail) =>
        new(false, null, 0, 0, 0, errorCode, detail);
}
