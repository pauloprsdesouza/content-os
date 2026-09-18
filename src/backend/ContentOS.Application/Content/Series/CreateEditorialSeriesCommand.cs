namespace ContentOS.Application.Content.Series;

public sealed record CreateEditorialSeriesCommand(
    string Format,
    string AreaId,
    string AreaName,
    bool AreaIsSubfield,
    int WindowDays,
    string Cadence,
    Guid OwnerUserId);
