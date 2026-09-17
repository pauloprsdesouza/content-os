using ContentOS.Application.Learning.Ports;
using ContentOS.Domain.Commerce;
using ContentOS.Domain.Learning;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Learning;

public sealed class OutcomesSummaryQuery(PlatformDbContext dbContext) : IOutcomesSummaryQuery
{
    public async Task<OutcomesSummary> GetAsync(CancellationToken cancellationToken = default)
    {
        var confirmedPurchases = await dbContext.Set<Purchase>()
            .AsNoTracking()
            .CountAsync(purchase => purchase.Status == PurchaseStatus.Confirmed, cancellationToken);
        var learners = await dbContext.Set<Learner>().AsNoTracking().CountAsync(cancellationToken);
        var enrollments = await dbContext.Set<Enrollment>().AsNoTracking().CountAsync(cancellationToken);
        var capstonesSubmitted = await dbContext.Set<Capstone>()
            .AsNoTracking()
            .CountAsync(
                capstone => capstone.Status == CapstoneStatus.Submitted
                    || capstone.Status == CapstoneStatus.Evaluated,
                cancellationToken);
        var evaluationsPassed = await dbContext.Set<Evaluation>()
            .AsNoTracking()
            .CountAsync(evaluation => evaluation.Passed, cancellationToken);
        var outcomesRecorded = await dbContext.Set<Outcome>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        return new OutcomesSummary(
            confirmedPurchases,
            learners,
            enrollments,
            capstonesSubmitted,
            evaluationsPassed,
            outcomesRecorded);
    }
}
