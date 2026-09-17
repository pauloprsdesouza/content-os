namespace ContentOS.Contracts.Research;

public sealed record ResearchJobListItemResponse(
    Guid Id,
    string Topic,
    string Status,
    DateTimeOffset UpdatedAt);
