namespace ContentOS.Application.Research.Ports;

public sealed record ResearchJobListItem(
    Guid Id,
    string Topic,
    string Status,
    DateTimeOffset UpdatedAt);
