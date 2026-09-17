using ContentOS.Domain.Knowledge;

namespace ContentOS.Application.Knowledge.Ports;

public sealed record ClaimReviewQueueItem(
    Guid Id,
    string Statement,
    ClaimStatus Status,
    decimal Confidence,
    long Version,
    DateTimeOffset UpdatedAt);
