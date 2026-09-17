namespace ContentOS.Contracts.Catalog;

public sealed record ReplaceCurriculumRequest(
    IReadOnlyList<Guid> ContentVersionIds);
