using ContentOS.Domain.Knowledge;
using Shouldly;

namespace ContentOS.Domain.UnitTests;

public sealed class ClaimTransitionTests
{
    [Fact]
    public void SubmitForReview_requires_evidence()
    {
        var claim = Claim.Create(
            Guid.CreateVersion7(),
            "Sample statement",
            ConfidenceScore.Create(0.8m),
            DateTimeOffset.UtcNow);

        var act = () => claim.SubmitForReview(DateTimeOffset.UtcNow);

        Should.Throw<InvalidOperationException>(act);
        claim.Status.ShouldBe(ClaimStatus.Draft);
    }

    [Fact]
    public void Approve_moves_pending_claim_to_approved()
    {
        var claim = CreatePendingClaim();
        var actor = Guid.CreateVersion7();
        var approvedAt = DateTimeOffset.UtcNow;

        claim.Approve(actor, claim.Version, approvedAt);

        claim.Status.ShouldBe(ClaimStatus.Approved);
        claim.ReviewedByUserId.ShouldBe(actor);
        claim.ReviewedAt.ShouldBe(approvedAt);
        claim.Version.ShouldBe(2);
        claim.RejectionReason.ShouldBeNull();
    }

    [Fact]
    public void Approve_without_evidence_is_rejected()
    {
        var claim = Claim.Create(
            Guid.CreateVersion7(),
            "Unsupported claim",
            ConfidenceScore.Create(0.5m),
            DateTimeOffset.UtcNow);

        // Force PendingReview illegally via linking then clearing is impossible;
        // draft cannot approve.
        var act = () => claim.Approve(Guid.CreateVersion7(), claim.Version, DateTimeOffset.UtcNow);

        Should.Throw<InvalidOperationException>(act);
        claim.Status.ShouldBe(ClaimStatus.Draft);
    }

    [Fact]
    public void Reject_requires_reason_and_increments_version()
    {
        var claim = CreatePendingClaim();
        var version = claim.Version;

        claim.Reject(
            Guid.CreateVersion7(),
            "Insufficient provenance",
            version,
            DateTimeOffset.UtcNow);

        claim.Status.ShouldBe(ClaimStatus.Rejected);
        claim.RejectionReason.ShouldBe("Insufficient provenance");
        claim.Version.ShouldBe(version + 1);
    }

    [Fact]
    public void Approve_with_stale_version_fails()
    {
        var claim = CreatePendingClaim();

        var act = () => claim.Approve(Guid.CreateVersion7(), claim.Version + 1, DateTimeOffset.UtcNow);

        Should.Throw<InvalidOperationException>(act);
        claim.Status.ShouldBe(ClaimStatus.PendingReview);
    }

    [Fact]
    public void Approved_claim_can_be_superseded()
    {
        var claim = CreatePendingClaim();
        claim.Approve(Guid.CreateVersion7(), claim.Version, DateTimeOffset.UtcNow);
        var successor = Guid.CreateVersion7();

        claim.MarkSuperseded(successor, DateTimeOffset.UtcNow);

        claim.Status.ShouldBe(ClaimStatus.Superseded);
        claim.SupersededByClaimId.ShouldBe(successor);
    }

    private static Claim CreatePendingClaim()
    {
        var now = DateTimeOffset.UtcNow;
        var claim = Claim.Create(
            Guid.CreateVersion7(),
            "Evidence-backed statement",
            ConfidenceScore.Create(0.91m),
            now);
        claim.LinkEvidence(Guid.CreateVersion7(), now);
        claim.SubmitForReview(now);
        return claim;
    }
}
