using ContentOS.Application.Content.Ports;
using ContentOS.Domain.Content;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Content;

public sealed class EditorialSeriesRepository(PlatformDbContext dbContext) : IEditorialSeriesRepository
{
    public void Add(EditorialSeries series) => dbContext.Set<EditorialSeries>().Add(series);

    public Task<EditorialSeries?> GetByIdAsync(Guid seriesId, CancellationToken cancellationToken = default) =>
        dbContext.Set<EditorialSeries>().FirstOrDefaultAsync(item => item.Id == seriesId, cancellationToken);

    public void Remove(EditorialSeries series) => dbContext.Set<EditorialSeries>().Remove(series);

    public async Task<IReadOnlyList<EditorialSeries>> ListForOwnerAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken = default) =>
        await dbContext.Set<EditorialSeries>()
            .AsNoTracking()
            .Where(item => item.OwnerUserId == ownerUserId)
            .OrderByDescending(item => item.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);
}
