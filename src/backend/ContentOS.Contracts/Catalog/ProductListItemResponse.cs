namespace ContentOS.Contracts.Catalog;

public sealed record ProductListItemResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset UpdatedAt);
