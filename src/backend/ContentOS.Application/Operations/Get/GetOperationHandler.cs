using ContentOS.Application.Operations.Ports;

namespace ContentOS.Application.Operations.Get;

public sealed class GetOperationHandler(IOperationRepository operations)
{
    public async Task<GetOperationResult> HandleAsync(
        GetOperationQuery query,
        CancellationToken cancellationToken = default)
    {
        var operation = await operations.GetByIdAsync(query.OperationId, cancellationToken);
        return operation is null
            ? GetOperationResult.NotFound()
            : GetOperationResult.Found(operation);
    }
}
