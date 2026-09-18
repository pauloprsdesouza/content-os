namespace ContentOS.Contracts.Content;

public sealed record CreateEditorialSeriesRequest(
    string Format,
    string AreaId,
    string AreaName,
    bool AreaIsSubfield,
    int WindowDays,
    string Cadence);
