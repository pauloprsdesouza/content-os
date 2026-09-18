using ContentOS.Application.Content.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Content;
using ContentOS.SharedKernel;

namespace ContentOS.Application.Content.Series;

public sealed class CreateEditorialSeriesHandler(
    IEditorialSeriesRepository series,
    ICadenceScheduler scheduler,
    IIdGenerator ids,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<CreateEditorialSeriesResult> HandleAsync(
        CreateEditorialSeriesCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!ContentFormat.TryParse(command.Format, out var format))
        {
            return CreateEditorialSeriesResult.Invalid("CONTENT_FORMAT_UNKNOWN");
        }

        if (!TryParseCadence(command.Cadence, out var cadence))
        {
            return CreateEditorialSeriesResult.Invalid("SERIES_CADENCE_UNKNOWN");
        }

        if (command.WindowDays is not (7 or 30 or 90))
        {
            return CreateEditorialSeriesResult.Invalid("TOPIC_WINDOW_INVALID");
        }

        EditorialSeries created;
        try
        {
            created = EditorialSeries.Create(
                ids.NewId(),
                format,
                command.AreaId,
                command.AreaName,
                command.AreaIsSubfield,
                command.WindowDays,
                cadence,
                command.OwnerUserId,
                clock.GetUtcNow());
        }
        catch (ArgumentException)
        {
            return CreateEditorialSeriesResult.Invalid("TOPIC_AREA_REQUIRED");
        }

        var next = created.NextAfter(clock.GetUtcNow());
        if (next is not null)
        {
            created.RememberNextCollection(next.Value);
        }

        series.Add(created);
        await changes.CommitAsync(cancellationToken);

        if (next is not null)
        {
            await scheduler.ScheduleAsync(created.Id, next.Value, cancellationToken);
        }

        return CreateEditorialSeriesResult.Created(created.Id);
    }

    private static bool TryParseCadence(string? value, out SeriesCadence cadence)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "manual":
                cadence = SeriesCadence.Manual;
                return true;
            case "weekly":
            case "semanal":
                cadence = SeriesCadence.Weekly;
                return true;
            case "monthly":
            case "mensal":
                cadence = SeriesCadence.Monthly;
                return true;
            default:
                cadence = SeriesCadence.Manual;
                return false;
        }
    }
}
