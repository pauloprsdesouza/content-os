namespace ContentOS.Application.Content.Ports;

public sealed record ContentUnitListItem(
    Guid Id,
    string Title,
    string? Brief,
    Guid? LatestVersionId,
    string? LatestVersionStatus,
    DateTimeOffset UpdatedAt);
