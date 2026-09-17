namespace ContentOS.Contracts.Knowledge;

public sealed record CreateClaimForReviewResponse(
    Guid ClaimId,
    Guid EvidenceId);
