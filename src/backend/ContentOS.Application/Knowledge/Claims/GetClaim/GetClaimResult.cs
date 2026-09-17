using ContentOS.Application.Knowledge.Ports;

namespace ContentOS.Application.Knowledge.Claims.GetClaim;

public sealed record GetClaimResult
{
    private GetClaimResult(bool found, ClaimReviewDetail? detail)
    {
        Found = found;
        Detail = detail;
    }

    public bool Found { get; }

    public ClaimReviewDetail? Detail { get; }

    public static GetClaimResult NotFound() => new(false, null);

    public static GetClaimResult From(ClaimReviewDetail detail) => new(true, detail);
}
