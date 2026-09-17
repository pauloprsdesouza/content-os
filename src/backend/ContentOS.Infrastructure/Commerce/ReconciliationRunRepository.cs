using ContentOS.Application.Commerce.Ports;
using ContentOS.Domain.Commerce;
using ContentOS.Infrastructure.Persistence;

namespace ContentOS.Infrastructure.Commerce;

public sealed class ReconciliationRunRepository(PlatformDbContext dbContext)
    : IReconciliationRunRepository
{
    public void Add(ReconciliationRun run) =>
        dbContext.Set<ReconciliationRun>().Add(run);
}
