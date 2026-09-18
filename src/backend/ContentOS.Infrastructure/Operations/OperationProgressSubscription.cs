using System.Threading.Channels;
using ContentOS.Application.Operations.Progress;

namespace ContentOS.Infrastructure.Operations;

public sealed class OperationProgressSubscription : IOperationProgressSubscription
{
    private readonly Channel<OperationProgressNotice> _channel;
    private readonly Action _release;
    private int _disposed;

    public OperationProgressSubscription(Channel<OperationProgressNotice> channel, Action release)
    {
        _channel = channel;
        _release = release;
    }

    public ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken) =>
        _channel.Reader.WaitToReadAsync(cancellationToken);

    public bool TryRead(out OperationProgressNotice notice) =>
        _channel.Reader.TryRead(out notice!);

    public ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            _release();
            _channel.Writer.TryComplete();
        }

        return ValueTask.CompletedTask;
    }
}
