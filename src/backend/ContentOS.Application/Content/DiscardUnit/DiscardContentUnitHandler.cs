using ContentOS.Application.Content.Ports;
using ContentOS.Application.Persistence;

namespace ContentOS.Application.Content.DiscardUnit;

public sealed class DiscardContentUnitHandler(
    IContentUnitRepository units,
    IContentVersionRepository versions,
    IContentPlacementQuery placement,
    IChangeCommitter changes)
{
    public async Task<DiscardOutcome> HandleAsync(
        DiscardContentUnitCommand command,
        CancellationToken cancellationToken = default)
    {
        var unit = await units.GetByIdAsync(command.ContentUnitId, cancellationToken);
        if (unit is null)
        {
            return DiscardOutcome.Missing("CONTENT_NOT_FOUND");
        }

        var versionIds = await versions.ListIdsByUnitAsync(command.ContentUnitId, cancellationToken);
        if (await placement.ReferencesAnyVersionAsync(versionIds, cancellationToken))
        {
            return DiscardOutcome.Blocked("CONTENT_IN_CURRICULUM");
        }

        await versions.DeleteByUnitAsync(command.ContentUnitId, cancellationToken);
        units.Remove(unit);
        await changes.CommitAsync(cancellationToken);
        return DiscardOutcome.Ok();
    }
}
