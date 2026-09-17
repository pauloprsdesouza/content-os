namespace ContentOS.Contracts.Research;

public sealed record ResearchFindingResponse(
    Guid Id,
    Guid ResearchJobId,
    string Statement,
    decimal Confidence,
    DateTimeOffset CreatedAt);
