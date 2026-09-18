namespace ContentOS.Application.Knowledge.Claims.Promote;

public sealed record PromoteResearchFindingItem(
    Guid FindingId,
    string Statement,
    decimal Confidence,
    Guid? SourceSnapshotId,
    string? Locator,
    string? ExtractionMethod);
