using ContentOS.Application.Knowledge.Ports;

namespace ContentOS.Application.Knowledge.Claims.GetClaim;

public sealed class GetClaimHandler(IClaimReviewQueueQuery claimsQuery)
{
    public async Task<GetClaimResult> HandleAsync(
        GetClaimQuery query,
        CancellationToken cancellationToken = default)
    {
        var detail = await claimsQuery.GetDetailAsync(query.ClaimId, cancellationToken);
        return detail is null
            ? GetClaimResult.NotFound()
            : GetClaimResult.From(detail);
    }
}
