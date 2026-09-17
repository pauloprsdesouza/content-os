namespace ContentOS.Contracts.Catalog;

public sealed record EditionDetailResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string Name,
    long Version,
    IReadOnlyList<CurriculumItemResponse> Curriculum,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
