using ContentOS.Domain.Knowledge;

namespace ContentOS.Application.Knowledge.Ports;

public interface IClaimReviewQueueQuery
{
    Task<ClaimReviewQueuePage> GetPendingAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ClaimReviewDetail?> GetDetailAsync(
        Guid claimId,
        CancellationToken cancellationToken = default);
}
