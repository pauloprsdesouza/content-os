using ContentOS.Contracts.Publication;
using ContentOS.Domain.Publication;

namespace ContentOS.Api.Publication;

public static class PublicationPackageResponseMapper
{
    public static PublicationPackageResponse Map(PublicationPackage package) =>
        new(
            package.Id,
            package.EditionId,
            package.Status.ToString(),
            package.RendererVersion,
            package.ManifestJson,
            package.ExportBlobSha256,
            package.ExportBlobLength,
            package.ConfirmedByUserId,
            package.ConfirmedAt,
            package.Version,
            package.CreatedAt,
            package.UpdatedAt);
}
