using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Messaging;

public sealed class ProcessedAiResultStore(PlatformDbContext dbContext)
{
    public Task<bool> ExistsAsync(string messageId, CancellationToken cancellationToken) =>
        dbContext.Set<InboxMessage>().AnyAsync(message => message.MessageId == messageId, cancellationToken);

    public void Remember(string messageId, string eventType) =>
        dbContext.Set<InboxMessage>().Add(new InboxMessage
        {
            Id = Guid.CreateVersion7(),
            MessageId = messageId,
            EventType = eventType,
            ProcessedAt = DateTimeOffset.UtcNow
        });
}
