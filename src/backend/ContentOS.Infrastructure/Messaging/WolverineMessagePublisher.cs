using ContentOS.Application.Messaging;
using Wolverine;

namespace ContentOS.Infrastructure.Messaging;

public sealed class WolverineMessagePublisher(IMessageBus messageBus) : IMessagePublisher
{
    public async Task PublishAsync<TMessage>(
        TMessage message,
        CancellationToken cancellationToken = default)
        where TMessage : notnull
    {
        cancellationToken.ThrowIfCancellationRequested();
        await messageBus.PublishAsync(message);
    }
}
