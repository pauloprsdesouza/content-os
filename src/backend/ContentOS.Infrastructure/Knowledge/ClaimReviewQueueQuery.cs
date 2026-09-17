using ContentOS.Application.Knowledge.Ports;
using ContentOS.Domain.Knowledge;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Knowledge;

public sealed class ClaimReviewQueueQuery(PlatformDbContext dbContext) : IClaimReviewQueueQuery
{
    public async Task<ClaimReviewQueuePage> GetPendingAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<Claim>()
            .AsNoTracking()
            .Where(claim => claim.Status == ClaimStatus.PendingReview)
            .OrderBy(claim => claim.UpdatedAt);

        var totalItems = await query.LongCountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(claim => new ClaimReviewQueueItem(
                claim.Id,
                claim.Statement,
                claim.Status,
                claim.Confidence.Value,
                claim.Version,
                claim.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new ClaimReviewQueuePage(items, page, pageSize, totalItems);
    }

    public async Task<ClaimReviewDetail?> GetDetailAsync(
        Guid claimId,
        CancellationToken cancellationToken = default)
    {
        var claim = await dbContext.Set<Claim>()
            .AsNoTracking()
            .Include(entity => entity.EvidenceLinks)
            .FirstOrDefaultAsync(entity => entity.Id == claimId, cancellationToken);

        if (claim is null)
        {
            return null;
        }

        var evidenceIds = claim.EvidenceLinks.Select(link => link.EvidenceId).ToArray();
        var evidenceRows = await dbContext.Set<Evidence>()
            .AsNoTracking()
            .Where(evidence => evidenceIds.Contains(evidence.Id))
            .ToListAsync(cancellationToken);

        var evidenceDetails = evidenceRows
            .Select(evidence => new ClaimEvidenceDetail(
                evidence.Id,
                evidence.SnapshotId,
                evidence.Locator.Value,
                evidence.ExtractionMethod,
                evidence.Confidence.Value))
            .ToArray();

        return new ClaimReviewDetail(
            claim.Id,
            claim.Statement,
            claim.Status,
            claim.Confidence.Value,
            claim.Version,
            claim.RejectionReason,
            claim.ReviewedByUserId,
            claim.ReviewedAt,
            claim.CreatedAt,
            claim.UpdatedAt,
            evidenceDetails);
    }
}
