namespace ContentOS.Contracts.Content;

public sealed record ContentUnitListItemResponse(
    Guid Id,
    string Title,
    string? Brief,
    Guid? LatestVersionId,
    string? LatestVersionStatus,
    DateTimeOffset UpdatedAt);
