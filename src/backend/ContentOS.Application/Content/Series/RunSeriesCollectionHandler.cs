using ContentOS.Application.Content.Ports;
using ContentOS.Application.Content.Topics;
using ContentOS.Application.Persistence;

namespace ContentOS.Application.Content.Series;

public sealed class RunSeriesCollectionHandler(
    IEditorialSeriesRepository series,
    StartTopicDiscoveryHandler discoveries,
    ICadenceScheduler scheduler,
    IChangeCommitter changes)
{
    public async Task<RunSeriesCollectionResult> HandleAsync(
        RunSeriesCollectionCommand command,
        CancellationToken cancellationToken = default)
    {
        var editorial = await series.GetByIdAsync(command.SeriesId, cancellationToken);
        if (editorial is null)
        {
            return RunSeriesCollectionResult.Invalid("SERIES_NOT_FOUND");
        }

        var started = await discoveries.HandleAsync(
            new StartTopicDiscoveryCommand(
                editorial.Format.Code,
                editorial.AreaId,
                editorial.AreaName,
                editorial.AreaIsSubfield,
                editorial.WindowDays,
                editorial.OwnerUserId,
                editorial.Id,
                command.ScheduledFor),
            cancellationToken);
        if (!started.IsSuccess || started.DiscoveryId is null)
        {
            return RunSeriesCollectionResult.Invalid(started.ErrorCode ?? "TOPIC_DISCOVERY_FAILED");
        }

        if (command.Reschedule && !started.IsDuplicate)
        {
            var next = editorial.NextAfter(command.ScheduledFor);
            if (next is not null)
            {
                editorial.RememberNextCollection(next.Value);
                await changes.CommitAsync(cancellationToken);
                await scheduler.ScheduleAsync(editorial.Id, next.Value, cancellationToken);
            }
        }

        return RunSeriesCollectionResult.Completed(started.DiscoveryId.Value);
    }
}
