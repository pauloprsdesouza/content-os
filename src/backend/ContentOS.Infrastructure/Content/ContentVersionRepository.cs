using ContentOS.Application.Content.Ports;
using ContentOS.Domain.Content;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Content;

public sealed class ContentVersionRepository(PlatformDbContext dbContext) : IContentVersionRepository
{
    public Task<ContentVersion?> GetByIdAsync(
        Guid contentVersionId,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<ContentVersion>()
            .FirstOrDefaultAsync(version => version.Id == contentVersionId, cancellationToken);

    public async Task<int> GetNextRevisionAsync(
        Guid contentUnitId,
        CancellationToken cancellationToken = default)
    {
        var max = await dbContext.Set<ContentVersion>()
            .Where(version => version.ContentUnitId == contentUnitId)
            .Select(version => (int?)version.Revision)
            .MaxAsync(cancellationToken);
        return (max ?? 0) + 1;
    }

    public void Add(ContentVersion version) => dbContext.Set<ContentVersion>().Add(version);
}
