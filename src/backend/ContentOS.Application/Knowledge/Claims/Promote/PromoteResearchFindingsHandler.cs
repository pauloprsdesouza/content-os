using ContentOS.Application.Knowledge.Ports;
using ContentOS.Domain.Knowledge;
using ContentOS.SharedKernel;

namespace ContentOS.Application.Knowledge.Claims.Promote;

public sealed class PromoteResearchFindingsHandler(
    IClaimRepository claims,
    IEvidenceRepository evidence,
    ISnapshotExistsQuery snapshots,
    IIdGenerator ids,
    TimeProvider clock)
{
    public async Task<PromoteResearchFindingsResult> HandleAsync(
        PromoteResearchFindingsCommand command,
        CancellationToken cancellationToken = default)
    {
        var promoted = 0;
        var needsEvidence = 0;
        var already = 0;
        var now = clock.GetUtcNow();

        foreach (var finding in command.Findings)
        {
            var existing = await claims.FindByOriginAsync(finding.FindingId, cancellationToken);
            if (existing is not null)
            {
                already++;
                continue;
            }

            if (finding.SourceSnapshotId is null
                || string.IsNullOrWhiteSpace(finding.Locator)
                || !await snapshots.ExistsAsync(finding.SourceSnapshotId.Value, cancellationToken))
            {
                needsEvidence++;
                continue;
            }

            var evidenceId = ids.NewId();
            var claimId = ids.NewId();
            var created = Evidence.Create(
                evidenceId,
                finding.SourceSnapshotId.Value,
                EvidenceLocator.Create(finding.Locator),
                finding.ExtractionMethod ?? "research-agent",
                ConfidenceScore.Create(finding.Confidence),
                now);
            evidence.Add(created);

            var claim = Claim.Create(
                claimId,
                finding.Statement,
                ConfidenceScore.Create(finding.Confidence),
                now);
            claim.AttachOrigin(finding.FindingId);
            claim.LinkEvidence(evidenceId, now);
            claim.SubmitForReview(now);
            claims.Add(claim);
            promoted++;
        }

        return new PromoteResearchFindingsResult(promoted, needsEvidence, already);
    }
}
