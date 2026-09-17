using ContentOS.Application.Operations.Ports;
using ContentOS.Domain.Operations;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Operations;

public sealed class OperationRepository(PlatformDbContext dbContext) : IOperationRepository
{
    public Task<Operation?> GetByIdAsync(
        Guid operationId,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<Operation>()
            .FirstOrDefaultAsync(operation => operation.Id == operationId, cancellationToken);

    public void Add(Operation operation) => dbContext.Set<Operation>().Add(operation);
}
