using ContentOS.Domain.Knowledge;

namespace ContentOS.Application.Knowledge.Ports;

public interface IClaimRepository
{
    Task<Claim?> GetByIdAsync(
        Guid claimId,
        CancellationToken cancellationToken = default);

    void Add(Claim claim);
}
