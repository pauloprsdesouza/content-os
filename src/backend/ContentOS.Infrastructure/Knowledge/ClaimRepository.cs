using ContentOS.Application.Knowledge.Ports;
using ContentOS.Domain.Knowledge;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Knowledge;

public sealed class ClaimRepository(PlatformDbContext dbContext) : IClaimRepository
{
    public Task<Claim?> GetByIdAsync(
        Guid claimId,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<Claim>()
            .Include(claim => claim.EvidenceLinks)
            .FirstOrDefaultAsync(claim => claim.Id == claimId, cancellationToken);

    public void Add(Claim claim) => dbContext.Set<Claim>().Add(claim);

    public Task<Claim?> FindByOriginAsync(
        Guid researchFindingId,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<Claim>()
            .FirstOrDefaultAsync(
                claim => claim.OriginResearchFindingId == researchFindingId,
                cancellationToken);
}
