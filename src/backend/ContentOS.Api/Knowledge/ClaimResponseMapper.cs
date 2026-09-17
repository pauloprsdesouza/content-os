using ContentOS.Application.Knowledge.Ports;
using ContentOS.Contracts.Knowledge;

namespace ContentOS.Api.Knowledge;

public static class ClaimResponseMapper
{
    public static ClaimResponse ToResponse(ClaimReviewDetail detail) =>
        new(
            detail.Id,
            detail.Statement,
            detail.Status.ToString(),
            detail.Confidence,
            detail.Version,
            detail.RejectionReason,
            detail.ReviewedByUserId,
            detail.ReviewedAt,
            detail.CreatedAt,
            detail.UpdatedAt,
            detail.Evidence
                .Select(evidence => new ClaimEvidenceResponse(
                    evidence.EvidenceId,
                    evidence.SnapshotId,
                    evidence.Locator,
                    evidence.ExtractionMethod,
                    evidence.Confidence))
                .ToArray());
}
