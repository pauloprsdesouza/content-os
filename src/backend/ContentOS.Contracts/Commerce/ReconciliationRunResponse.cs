namespace ContentOS.Contracts.Commerce;

public sealed record ReconciliationRunResponse(
    Guid RunId,
    int ProcessedCount,
    int ConfirmedCount,
    int IgnoredCount);
