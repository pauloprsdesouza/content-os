namespace ContentOS.Application.Content.Ports;

public sealed record ContentUnitListItem(
    Guid Id,
    string Title,
    string? Brief,
    string Format,
    Guid? LatestVersionId,
    string? LatestVersionStatus,
    DateTimeOffset UpdatedAt,
    Guid? ProductId);
