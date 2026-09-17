using ContentOS.Application.Knowledge.Ports;

namespace ContentOS.Application.Knowledge.Claims.GetReviewQueue;

public sealed class GetClaimReviewQueueHandler(IClaimReviewQueueQuery claimsQuery)
{
    public async Task<GetClaimReviewQueueResult> HandleAsync(
        GetClaimReviewQueueQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var result = await claimsQuery.GetPendingAsync(page, pageSize, cancellationToken);
        return new GetClaimReviewQueueResult(result);
    }
}
