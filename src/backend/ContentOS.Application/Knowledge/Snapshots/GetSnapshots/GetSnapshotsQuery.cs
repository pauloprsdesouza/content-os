namespace ContentOS.Application.Knowledge.Snapshots.GetSnapshots;

public sealed record GetSnapshotsQuery(Guid SourceId, int Page, int PageSize);
