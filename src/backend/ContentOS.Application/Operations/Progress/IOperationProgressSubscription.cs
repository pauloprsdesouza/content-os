namespace ContentOS.Application.Operations.Progress;

public interface IOperationProgressSubscription : IAsyncDisposable
{
    ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken);

    bool TryRead(out OperationProgressNotice notice);
}
