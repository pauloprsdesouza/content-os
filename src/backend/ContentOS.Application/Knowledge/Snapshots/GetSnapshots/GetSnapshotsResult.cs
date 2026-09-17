using ContentOS.Application.Knowledge.Ports;

namespace ContentOS.Application.Knowledge.Snapshots.GetSnapshots;

public sealed record GetSnapshotsResult
{
    private GetSnapshotsResult(bool sourceFound, SnapshotsPage? page)
    {
        SourceFound = sourceFound;
        Page = page;
    }

    public bool SourceFound { get; }

    public SnapshotsPage? Page { get; }

    public static GetSnapshotsResult NotFound() => new(false, null);

    public static GetSnapshotsResult Found(SnapshotsPage page) => new(true, page);
}
