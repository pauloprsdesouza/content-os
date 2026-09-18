namespace ContentOS.Contracts.Content;

public sealed record ContentUnitListItemResponse(
    Guid Id,
    string Title,
    string? Brief,
    string Format,
    Guid? LatestVersionId,
    string? LatestVersionStatus,
    DateTimeOffset UpdatedAt,
    Guid? ProductId);
