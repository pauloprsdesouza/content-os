using ContentOS.Application.Content.Ports;
using ContentOS.Domain.Catalog;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Content;

public sealed class ContentPlacementQuery(PlatformDbContext dbContext) : IContentPlacementQuery
{
    public Task<bool> ReferencesAnyVersionAsync(
        IReadOnlyCollection<Guid> contentVersionIds,
        CancellationToken cancellationToken = default)
    {
        if (contentVersionIds.Count == 0)
        {
            return Task.FromResult(false);
        }

        return dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .AnyAsync(item => contentVersionIds.Contains(item.ContentVersionId), cancellationToken);
    }
}
