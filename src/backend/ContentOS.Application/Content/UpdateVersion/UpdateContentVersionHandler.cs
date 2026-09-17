using ContentOS.Application.Content.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Content;

namespace ContentOS.Application.Content.UpdateVersion;

public sealed class UpdateContentVersionHandler(
    IContentVersionRepository versions,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<UpdateContentVersionResult> HandleAsync(
        UpdateContentVersionCommand command,
        CancellationToken cancellationToken = default)
    {
        var version = await versions.GetByIdAsync(command.ContentVersionId, cancellationToken);
        if (version is null)
        {
            return UpdateContentVersionResult.NotFound();
        }

        if (version.Version != command.ExpectedVersion)
        {
            return UpdateContentVersionResult.ConcurrencyConflict(version.Version);
        }

        try
        {
            version.UpdateDraftBody(
                command.BodyMarkdown,
                command.ExpectedVersion,
                clock.GetUtcNow());
        }
        catch (InvalidOperationException) when (
            version.Status is not (ContentVersionStatus.Draft or ContentVersionStatus.ChangesRequested))
        {
            return UpdateContentVersionResult.InvalidState("CONTENT_VERSION_IMMUTABLE");
        }
        catch (ArgumentException)
        {
            return UpdateContentVersionResult.InvalidState("CONTENT_BODY_REQUIRED");
        }

        await changes.CommitAsync(cancellationToken);
        return UpdateContentVersionResult.Updated(version.Version);
    }
}
