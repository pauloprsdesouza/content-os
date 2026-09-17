using ContentOS.Domain.Operations;

namespace ContentOS.Application.Operations.Ports;

public interface IOperationRepository
{
    Task<Operation?> GetByIdAsync(
        Guid operationId,
        CancellationToken cancellationToken = default);

    void Add(Operation operation);
}
