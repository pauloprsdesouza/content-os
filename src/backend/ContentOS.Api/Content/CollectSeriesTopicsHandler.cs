using ContentOS.Application.Content.Series;

namespace ContentOS.Api.Content;

public static class CollectSeriesTopicsHandler
{
    public static Task Handle(
        CollectSeriesTopics message,
        RunSeriesCollectionHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(
            new RunSeriesCollectionCommand(message.SeriesId, message.ScheduledFor, Reschedule: true),
            cancellationToken);
}
