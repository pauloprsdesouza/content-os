namespace ContentOS.Application.Content.Series;

public interface ICadenceScheduler
{
    Task ScheduleAsync(Guid seriesId, DateTimeOffset runAt, CancellationToken cancellationToken = default);
}
