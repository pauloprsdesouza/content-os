using ContentOS.Application.Persistence;
using ContentOS.Application.Publication.Ports;
using ContentOS.Domain.Publication;

namespace ContentOS.Application.Publication.ConfirmPublication;

public sealed class ConfirmPublicationHandler(
    IPublicationPackageRepository packages,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<ConfirmPublicationResult> HandleAsync(
        ConfirmPublicationCommand command,
        CancellationToken cancellationToken = default)
    {
        var package = await packages.GetByIdAsync(command.PackageId, cancellationToken);
        if (package is null)
        {
            return ConfirmPublicationResult.NotFound();
        }

        if (package.Version != command.ExpectedVersion)
        {
            return ConfirmPublicationResult.ConcurrencyConflict(package.Version);
        }

        try
        {
            package.ConfirmPublication(
                command.ActorUserId,
                command.ExpectedVersion,
                clock.GetUtcNow());
        }
        catch (InvalidOperationException) when (package.Status != PublicationPackageStatus.Exported)
        {
            return ConfirmPublicationResult.InvalidState("PUBLICATION_PACKAGE_INVALID_STATE");
        }
        catch (InvalidOperationException)
        {
            return ConfirmPublicationResult.ConcurrencyConflict(package.Version);
        }

        await changes.CommitAsync(cancellationToken);
        return ConfirmPublicationResult.Confirmed(package.Version);
    }
}
