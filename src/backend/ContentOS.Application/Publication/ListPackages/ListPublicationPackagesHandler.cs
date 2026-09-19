using ContentOS.Application.Publication.Ports;

namespace ContentOS.Application.Publication.ListPackages;

public sealed class ListPublicationPackagesHandler(IPublicationPackageRepository packages)
{
    public async Task<ListPublicationPackagesResult> HandleAsync(
        ListPublicationPackagesQuery query,
        CancellationToken cancellationToken = default)
    {
        var items = await packages.ListByEditionAsync(query.EditionId, cancellationToken);
        return new ListPublicationPackagesResult(items);
    }
}
