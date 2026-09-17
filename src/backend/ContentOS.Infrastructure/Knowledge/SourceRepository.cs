using ContentOS.Application.Knowledge.Ports;
using ContentOS.Domain.Knowledge;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Knowledge;

public sealed class SourceRepository(PlatformDbContext dbContext) : ISourceRepository
{
    public Task<bool> CanonicalUriExistsAsync(
        SourceUri canonicalUri,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<Source>()
            .AsNoTracking()
            .AnyAsync(source => source.CanonicalUri == canonicalUri, cancellationToken);

    public Task<Source?> GetByIdAsync(
        Guid sourceId,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<Source>()
            .FirstOrDefaultAsync(source => source.Id == sourceId, cancellationToken);

    public void Add(Source source) => dbContext.Set<Source>().Add(source);
}
