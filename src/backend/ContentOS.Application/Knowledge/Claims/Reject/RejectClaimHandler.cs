using ContentOS.Application.Knowledge.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Knowledge;

namespace ContentOS.Application.Knowledge.Claims.Reject;

public sealed class RejectClaimHandler(
    IClaimRepository claims,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<RejectClaimResult> HandleAsync(
        RejectClaimCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            return RejectClaimResult.InvalidState("CLAIM_REJECTION_REASON_REQUIRED");
        }

        var claim = await claims.GetByIdAsync(command.ClaimId, cancellationToken);
        if (claim is null)
        {
            return RejectClaimResult.NotFound();
        }

        if (claim.Version != command.ExpectedVersion)
        {
            return RejectClaimResult.ConcurrencyConflict(claim.Version);
        }

        try
        {
            claim.Reject(
                command.ActorUserId,
                command.Reason,
                command.ExpectedVersion,
                clock.GetUtcNow());
        }
        catch (InvalidOperationException) when (claim.Status != ClaimStatus.PendingReview)
        {
            return RejectClaimResult.InvalidState("CLAIM_INVALID_STATE");
        }

        await changes.CommitAsync(cancellationToken);
        return RejectClaimResult.Rejected(claim.Version);
    }
}
