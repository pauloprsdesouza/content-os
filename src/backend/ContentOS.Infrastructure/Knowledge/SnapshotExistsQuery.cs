using ContentOS.Application.Knowledge.Ports;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Knowledge;

public sealed class SnapshotExistsQuery(PlatformDbContext dbContext) : ISnapshotExistsQuery
{
    public Task<bool> ExistsAsync(Guid snapshotId, CancellationToken cancellationToken = default) =>
        dbContext.SourceSnapshots.AsNoTracking()
            .AnyAsync(snapshot => snapshot.Id == snapshotId, cancellationToken);
}
