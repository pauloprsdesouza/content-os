namespace ContentOS.Application.Catalog.Ports;

public sealed record EditionDetailReadModel(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string Name,
    long Version,
    IReadOnlyList<EditionCurriculumItemReadModel> Curriculum,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
