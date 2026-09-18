namespace ContentOS.Application.Content.Topics;

public sealed record StartTopicDiscoveryCommand(
    string Format,
    string AreaId,
    string AreaName,
    bool AreaIsSubfield,
    int WindowDays,
    Guid OwnerUserId,
    Guid? SeriesId,
    DateTimeOffset? ScheduledFor);
