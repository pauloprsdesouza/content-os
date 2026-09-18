using ContentOS.Application.Knowledge.Ports;
using ContentOS.Domain.Knowledge;

namespace ContentOS.Application.UnitTests;

public sealed class InMemoryClaimRepository : IClaimRepository
{
    public List<Claim> Items { get; } = [];

    public Task<Claim?> GetByIdAsync(Guid claimId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(claim => claim.Id == claimId));

    public void Add(Claim claim) => Items.Add(claim);

    public Task<Claim?> FindByOriginAsync(Guid researchFindingId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(claim => claim.OriginResearchFindingId == researchFindingId));
}
