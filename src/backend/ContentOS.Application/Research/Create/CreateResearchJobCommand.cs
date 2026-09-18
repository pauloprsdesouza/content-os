namespace ContentOS.Application.Research.Create;

public sealed record CreateResearchJobCommand(
    string Topic,
    string? ScopeNotes,
    Guid RequestedByUserId,
    Guid SourceSnapshotId);
