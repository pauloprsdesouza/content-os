using ContentOS.Application.Publication.Ports;

namespace ContentOS.Application.Publication.GetPackage;

public sealed class GetPublicationPackageHandler(IPublicationPackageRepository packages)
{
    public async Task<GetPublicationPackageResult> HandleAsync(
        GetPublicationPackageQuery query,
        CancellationToken cancellationToken = default)
    {
        var package = await packages.GetByIdAsync(query.PackageId, cancellationToken);
        return package is null
            ? GetPublicationPackageResult.NotFound()
            : GetPublicationPackageResult.Found(package);
    }
}
