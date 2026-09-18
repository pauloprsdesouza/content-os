namespace ContentOS.Application.Catalog.Ports;

public sealed record ProductListItem(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset UpdatedAt,
    Guid? EditionId);
