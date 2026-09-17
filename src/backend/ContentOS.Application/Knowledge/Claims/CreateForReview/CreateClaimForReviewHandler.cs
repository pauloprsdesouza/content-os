using ContentOS.Application.Knowledge.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Knowledge;
using ContentOS.SharedKernel;

namespace ContentOS.Application.Knowledge.Claims.CreateForReview;

public sealed class CreateClaimForReviewHandler(
    ISourceSnapshotRepository snapshots,
    IEvidenceRepository evidence,
    IClaimRepository claims,
    IIdGenerator ids,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<CreateClaimForReviewResult> HandleAsync(
        CreateClaimForReviewCommand command,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await snapshots.GetByIdAsync(command.SnapshotId, cancellationToken);
        if (snapshot is null)
        {
            return CreateClaimForReviewResult.SnapshotNotFound();
        }

        ConfidenceScore confidence;
        EvidenceLocator locator;
        try
        {
            confidence = ConfidenceScore.Create(command.Confidence);
            locator = EvidenceLocator.Create(command.EvidenceLocator);
        }
        catch (ArgumentException)
        {
            return CreateClaimForReviewResult.InvalidInput("CLAIM_INVALID_INPUT");
        }

        var now = clock.GetUtcNow();
        var evidenceEntity = Evidence.Create(
            ids.NewId(),
            snapshot.Id,
            locator,
            command.ExtractionMethod,
            confidence,
            now);

        var claim = Claim.Create(
            ids.NewId(),
            command.Statement,
            confidence,
            now);
        claim.LinkEvidence(evidenceEntity.Id, now);
        claim.SubmitForReview(now);

        evidence.Add(evidenceEntity);
        claims.Add(claim);
        await changes.CommitAsync(cancellationToken);
        return CreateClaimForReviewResult.Created(claim.Id, evidenceEntity.Id);
    }
}
