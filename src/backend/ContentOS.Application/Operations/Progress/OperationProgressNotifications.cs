using ContentOS.Domain.Operations;

namespace ContentOS.Application.Operations.Progress;

public static class OperationProgressNotifications
{
    public static void Notify(IOperationProgress progress, Operation operation) =>
        progress.Publish(
            new OperationProgressNotice(
                operation.Id,
                operation.Status.ToString(),
                operation.ErrorMessage,
                operation.UpdatedAt));
}
