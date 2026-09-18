using ContentOS.Application.Dashboard;
using ContentOS.Domain.Commerce;
using ContentOS.Domain.Content;
using ContentOS.Domain.Knowledge;
using ContentOS.Domain.Publication;
using ContentOS.Domain.Research;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Dashboard;

public sealed class EfDashboardSummaryQuery(PlatformDbContext dbContext, TimeProvider clock)
    : IDashboardSummaryQuery
{
    public async Task<DashboardSummary> GetAsync(CancellationToken cancellationToken = default)
    {
        var terminalResearch = new[]
        {
            ResearchJobStatus.Completed,
            ResearchJobStatus.Failed,
            ResearchJobStatus.Cancelled
        };

        return new DashboardSummary(
            await dbContext.Sources.AsNoTracking().LongCountAsync(cancellationToken),
            await dbContext.Claims.AsNoTracking()
                .LongCountAsync(claim => claim.Status == ClaimStatus.PendingReview, cancellationToken),
            await dbContext.ResearchJobs.AsNoTracking()
                .LongCountAsync(job => !terminalResearch.Contains(job.Status), cancellationToken),
            await dbContext.ContentVersions.AsNoTracking()
                .LongCountAsync(
                    version => version.Status == ContentVersionStatus.PendingHumanApproval,
                    cancellationToken),
            await dbContext.PublicationPackages.AsNoTracking()
                .LongCountAsync(
                    package => package.Status == PublicationPackageStatus.Exported,
                    cancellationToken),
            await dbContext.Purchases.AsNoTracking()
                .LongCountAsync(purchase => purchase.Status == PurchaseStatus.Confirmed, cancellationToken),
            await dbContext.Learners.AsNoTracking().LongCountAsync(cancellationToken),
            await dbContext.Outcomes.AsNoTracking()
                .LongCountAsync(outcome => outcome.Passed, cancellationToken),
            clock.GetUtcNow());
    }
}
