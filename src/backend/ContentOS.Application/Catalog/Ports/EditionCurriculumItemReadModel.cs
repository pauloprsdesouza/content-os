namespace ContentOS.Application.Catalog.Ports;

public sealed record EditionCurriculumItemReadModel(
    int Position,
    Guid ContentVersionId);
