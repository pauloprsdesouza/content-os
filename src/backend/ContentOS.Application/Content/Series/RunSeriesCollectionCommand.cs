namespace ContentOS.Application.Content.Series;

public sealed record RunSeriesCollectionCommand(
    Guid SeriesId,
    DateTimeOffset ScheduledFor,
    bool Reschedule);
