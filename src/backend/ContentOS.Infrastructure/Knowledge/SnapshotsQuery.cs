using ContentOS.Application.Knowledge.Ports;
using ContentOS.Domain.Knowledge;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Knowledge;

public sealed class SnapshotsQuery(PlatformDbContext dbContext) : ISnapshotsQuery
{
    public async Task<SnapshotsPage> GetBySourceAsync(
        Guid sourceId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<SourceSnapshot>()
            .AsNoTracking()
            .Where(snapshot => snapshot.SourceId == sourceId)
            .OrderByDescending(snapshot => snapshot.CapturedAt);

        var totalItems = await query.LongCountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(snapshot => new SnapshotListItem(
                snapshot.Id,
                snapshot.SourceId,
                snapshot.ContentHash.ToString(),
                snapshot.MediaType,
                snapshot.ByteLength,
                snapshot.CapturedAt))
            .ToListAsync(cancellationToken);

        return new SnapshotsPage(items, page, pageSize, totalItems);
    }
}
