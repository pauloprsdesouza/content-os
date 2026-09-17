using ContentOS.Application.Knowledge.Ports;
using ContentOS.Domain.Knowledge;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Knowledge;

public sealed class SnapshotByHashQuery(PlatformDbContext dbContext) : ISnapshotByHashQuery
{
    public async Task<SnapshotByHashDetail?> GetByHashAsync(
        string contentHash,
        CancellationToken cancellationToken = default)
    {
        ContentHash hash;
        try
        {
            hash = ContentHash.Create(contentHash);
        }
        catch (ArgumentException)
        {
            return null;
        }

        var snapshot = await dbContext.Set<SourceSnapshot>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.ContentHash == hash,
                cancellationToken);

        if (snapshot is null)
        {
            return null;
        }

        return new SnapshotByHashDetail(
            snapshot.Id,
            snapshot.SourceId,
            snapshot.ContentHash.Value,
            snapshot.MediaType,
            snapshot.ByteLength,
            snapshot.CapturedAt);
    }
}
