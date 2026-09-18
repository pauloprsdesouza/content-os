namespace ContentOS.Contracts.Content;

public sealed record EditorialSeriesResponse(
    Guid Id,
    string Format,
    string AreaId,
    string AreaName,
    bool AreaIsSubfield,
    int WindowDays,
    string Cadence,
    DateTimeOffset? NextCollectionAt,
    DateTimeOffset CreatedAt);
