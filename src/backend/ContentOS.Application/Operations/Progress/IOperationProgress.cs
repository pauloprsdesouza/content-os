namespace ContentOS.Application.Operations.Progress;

public interface IOperationProgress
{
    void Publish(OperationProgressNotice notice);

    IOperationProgressSubscription Subscribe(Guid operationId);
}
