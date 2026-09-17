using ContentOS.Application.Knowledge.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Knowledge;

namespace ContentOS.Application.Knowledge.Claims.Approve;

public sealed class ApproveClaimHandler(
    IClaimRepository claims,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<ApproveClaimResult> HandleAsync(
        ApproveClaimCommand command,
        CancellationToken cancellationToken = default)
    {
        var claim = await claims.GetByIdAsync(command.ClaimId, cancellationToken);
        if (claim is null)
        {
            return ApproveClaimResult.NotFound();
        }

        if (claim.Version != command.ExpectedVersion)
        {
            return ApproveClaimResult.ConcurrencyConflict(claim.Version);
        }

        try
        {
            claim.Approve(
                command.ActorUserId,
                command.ExpectedVersion,
                clock.GetUtcNow());
        }
        catch (InvalidOperationException) when (claim.Status != ClaimStatus.PendingReview)
        {
            return ApproveClaimResult.InvalidState("CLAIM_INVALID_STATE");
        }
        catch (InvalidOperationException) when (claim.EvidenceLinks.Count == 0)
        {
            return ApproveClaimResult.InvalidState("CLAIM_MISSING_EVIDENCE");
        }

        await changes.CommitAsync(cancellationToken);
        return ApproveClaimResult.Approved(claim.Version);
    }
}
