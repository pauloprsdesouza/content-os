using ContentOS.Application.Content.Ports;
using ContentOS.Domain.Content;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Content;

public sealed class ContentUnitsQuery(PlatformDbContext dbContext) : IContentUnitsQuery
{
    public async Task<ContentUnitsPage> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var unitsQuery = dbContext.Set<ContentUnit>().AsNoTracking();
        var total = await unitsQuery.LongCountAsync(cancellationToken);

        var units = await unitsQuery
            .OrderByDescending(unit => unit.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var unitIds = units.Select(unit => unit.Id).ToList();
        var versions = await dbContext.Set<ContentVersion>()
            .AsNoTracking()
            .Where(version => unitIds.Contains(version.ContentUnitId))
            .ToListAsync(cancellationToken);

        var latestByUnit = versions
            .GroupBy(version => version.ContentUnitId)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(version => version.Revision).First());

        var items = units
            .Select(unit =>
            {
                latestByUnit.TryGetValue(unit.Id, out var latest);
                return new ContentUnitListItem(
                    unit.Id,
                    unit.Title,
                    unit.Brief,
                    latest?.Id,
                    latest?.Status.ToString(),
                    unit.UpdatedAt);
            })
            .ToList();

        return new ContentUnitsPage(items, total);
    }
}
