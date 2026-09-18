using System.Collections.Concurrent;
using System.Threading.Channels;
using ContentOS.Application.Operations.Progress;

namespace ContentOS.Infrastructure.Operations;

public sealed class OperationProgressBroker : IOperationProgress
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, Channel<OperationProgressNotice>>> _listeners = new();

    public void Publish(OperationProgressNotice notice)
    {
        if (!_listeners.TryGetValue(notice.OperationId, out var listeners))
        {
            return;
        }

        foreach (var channel in listeners.Values)
        {
            channel.Writer.TryWrite(notice);
        }
    }

    public IOperationProgressSubscription Subscribe(Guid operationId)
    {
        var subscriptionId = Guid.NewGuid();
        var channel = Channel.CreateBounded<OperationProgressNotice>(
            new BoundedChannelOptions(16)
            {
                SingleReader = true,
                FullMode = BoundedChannelFullMode.DropOldest
            });
        var bucket = _listeners.GetOrAdd(operationId, _ => new ConcurrentDictionary<Guid, Channel<OperationProgressNotice>>());
        bucket[subscriptionId] = channel;

        return new OperationProgressSubscription(channel, () => Release(operationId, subscriptionId, bucket));
    }

    private void Release(
        Guid operationId,
        Guid subscriptionId,
        ConcurrentDictionary<Guid, Channel<OperationProgressNotice>> bucket)
    {
        bucket.TryRemove(subscriptionId, out _);
        if (bucket.IsEmpty)
        {
            _listeners.TryRemove(new KeyValuePair<Guid, ConcurrentDictionary<Guid, Channel<OperationProgressNotice>>>(operationId, bucket));
        }
    }
}
