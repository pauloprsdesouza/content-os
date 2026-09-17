using ContentOS.Application.Knowledge.Ports;
using ContentOS.Domain.Knowledge;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Knowledge;

public sealed class SourceSnapshotRepository(PlatformDbContext dbContext) : ISourceSnapshotRepository
{
    public Task<SourceSnapshot?> GetByIdAsync(
        Guid snapshotId,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<SourceSnapshot>()
            .FirstOrDefaultAsync(snapshot => snapshot.Id == snapshotId, cancellationToken);

    public void Add(SourceSnapshot snapshot) =>
        dbContext.Set<SourceSnapshot>().Add(snapshot);
}
