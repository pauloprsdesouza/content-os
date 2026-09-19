using ContentOS.Domain.Publication;

namespace ContentOS.Application.Publication.Ports;

public interface IPublicationPackageRepository
{
    Task<PublicationPackage?> GetByIdAsync(
        Guid packageId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PublicationPackage>> ListByEditionAsync(
        Guid editionId,
        CancellationToken cancellationToken = default);

    void Add(PublicationPackage package);
}
