namespace ContentOS.Contracts.Knowledge;

public sealed record ClaimQueueItemResponse(
    Guid Id,
    string Statement,
    string Status,
    decimal Confidence,
    long Version,
    DateTimeOffset UpdatedAt);
