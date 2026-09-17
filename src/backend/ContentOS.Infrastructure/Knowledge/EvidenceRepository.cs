using ContentOS.Application.Knowledge.Ports;
using ContentOS.Domain.Knowledge;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Knowledge;

public sealed class EvidenceRepository(PlatformDbContext dbContext) : IEvidenceRepository
{
    public async Task<IReadOnlyList<Evidence>> GetByIdsAsync(
        IReadOnlyCollection<Guid> evidenceIds,
        CancellationToken cancellationToken = default)
    {
        if (evidenceIds.Count == 0)
        {
            return [];
        }

        return await dbContext.Set<Evidence>()
            .Where(evidence => evidenceIds.Contains(evidence.Id))
            .ToListAsync(cancellationToken);
    }

    public void Add(Evidence evidence) => dbContext.Set<Evidence>().Add(evidence);
}
