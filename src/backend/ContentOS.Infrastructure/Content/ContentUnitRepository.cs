using ContentOS.Application.Content.Ports;
using ContentOS.Domain.Content;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Content;

public sealed class ContentUnitRepository(PlatformDbContext dbContext) : IContentUnitRepository
{
    public Task<ContentUnit?> GetByIdAsync(
        Guid contentUnitId,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<ContentUnit>()
            .FirstOrDefaultAsync(unit => unit.Id == contentUnitId, cancellationToken);

    public void Add(ContentUnit unit) => dbContext.Set<ContentUnit>().Add(unit);

    public void Remove(ContentUnit unit) => dbContext.Set<ContentUnit>().Remove(unit);
}
