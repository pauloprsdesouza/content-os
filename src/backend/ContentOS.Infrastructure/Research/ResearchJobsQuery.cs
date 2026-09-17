using ContentOS.Application.Research.Ports;
using ContentOS.Domain.Research;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Research;

public sealed class ResearchJobsQuery(PlatformDbContext dbContext) : IResearchJobsQuery
{
    public async Task<ResearchJobsPage> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<ResearchJob>().AsNoTracking();
        var total = await query.LongCountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(job => job.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(job => new ResearchJobListItem(
                job.Id,
                job.Topic,
                job.Status.ToString(),
                job.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new ResearchJobsPage(items, total);
    }
}
