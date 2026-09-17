using ContentOS.Domain.Commerce;

namespace ContentOS.Application.Commerce.Ports;

public interface IReconciliationRunRepository
{
    void Add(ReconciliationRun run);
}
