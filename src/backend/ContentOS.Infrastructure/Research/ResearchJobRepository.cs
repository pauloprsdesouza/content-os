using ContentOS.Application.Research.Ports;
using ContentOS.Domain.Research;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Research;

public sealed class ResearchJobRepository(PlatformDbContext dbContext) : IResearchJobRepository
{
    public Task<ResearchJob?> GetByIdAsync(
        Guid researchJobId,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<ResearchJob>()
            .Include(job => job.Findings)
            .FirstOrDefaultAsync(job => job.Id == researchJobId, cancellationToken);

    public void Add(ResearchJob job) => dbContext.Set<ResearchJob>().Add(job);
}
