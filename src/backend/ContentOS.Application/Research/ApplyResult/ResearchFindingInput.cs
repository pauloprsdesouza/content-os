namespace ContentOS.Application.Research.ApplyResult;

public sealed record ResearchFindingInput(
    string Statement,
    decimal Confidence,
    Guid? SourceSnapshotId = null,
    string? Locator = null,
    string? ExtractionMethod = null,
    Guid? FindingId = null);
