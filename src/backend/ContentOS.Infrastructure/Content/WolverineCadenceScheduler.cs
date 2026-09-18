using ContentOS.Application.Content.Series;
using Wolverine;

namespace ContentOS.Infrastructure.Content;

public sealed class WolverineCadenceScheduler(IMessageBus messageBus) : ICadenceScheduler
{
    public async Task ScheduleAsync(
        Guid seriesId,
        DateTimeOffset runAt,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await messageBus.ScheduleAsync(new CollectSeriesTopics(seriesId, runAt), runAt);
    }
}
