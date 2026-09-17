using ContentOS.Application.Knowledge.Ports;
using ContentOS.Domain.Knowledge;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Knowledge;

public sealed class SourcesQuery(PlatformDbContext dbContext) : ISourcesQuery
{
    public async Task<SourcesPage> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<Source>().AsNoTracking().OrderByDescending(source => source.UpdatedAt);
        var totalItems = await query.LongCountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(source => new SourceListItem(
                source.Id,
                source.DisplayName,
                source.CanonicalUri.ToString(),
                source.Kind,
                source.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new SourcesPage(items, page, pageSize, totalItems);
    }
}
