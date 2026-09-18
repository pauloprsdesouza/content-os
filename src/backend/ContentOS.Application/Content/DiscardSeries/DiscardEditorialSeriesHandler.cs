using ContentOS.Application.Content.Ports;
using ContentOS.Application.Persistence;

namespace ContentOS.Application.Content.DiscardSeries;

public sealed class DiscardEditorialSeriesHandler(
    IEditorialSeriesRepository series,
    IChangeCommitter changes)
{
    public async Task<DiscardOutcome> HandleAsync(
        DiscardEditorialSeriesCommand command,
        CancellationToken cancellationToken = default)
    {
        var editorial = await series.GetByIdAsync(command.SeriesId, cancellationToken);
        if (editorial is null)
        {
            return DiscardOutcome.Missing("SERIES_NOT_FOUND");
        }

        series.Remove(editorial);
        await changes.CommitAsync(cancellationToken);
        return DiscardOutcome.Ok();
    }
}
