namespace ContentOS.Application.Catalog.ReplaceCurriculum;

public sealed record ReplaceCurriculumCommand(
    Guid EditionId,
    IReadOnlyList<Guid> ContentVersionIds,
    long ExpectedVersion);
