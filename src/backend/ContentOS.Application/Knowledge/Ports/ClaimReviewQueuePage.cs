namespace ContentOS.Application.Knowledge.Ports;

public sealed record ClaimReviewQueuePage(
    IReadOnlyList<ClaimReviewQueueItem> Items,
    int Page,
    int PageSize,
    long TotalItems);
