using System.Text;
using ContentOS.Application.Blobs;
using ContentOS.Application.Persistence;
using ContentOS.Application.Publication.Ports;
using ContentOS.Domain.Publication;

namespace ContentOS.Application.Publication.ExportPackage;

public sealed class ExportPublicationPackageHandler(
    IPublicationPackageRepository packages,
    IBlobStore blobs,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<ExportPublicationPackageResult> HandleAsync(
        ExportPublicationPackageCommand command,
        CancellationToken cancellationToken = default)
    {
        var package = await packages.GetByIdAsync(command.PackageId, cancellationToken);
        if (package is null)
        {
            return ExportPublicationPackageResult.NotFound();
        }

        if (package.Status != PublicationPackageStatus.ReadyForExport)
        {
            return ExportPublicationPackageResult.InvalidState(
                "PUBLICATION_PACKAGE_INVALID_STATE");
        }

        if (string.IsNullOrWhiteSpace(package.ManifestJson))
        {
            return ExportPublicationPackageResult.InvalidState(
                "PUBLICATION_PACKAGE_MANIFEST_MISSING");
        }

        var exportDocument = BuildExportDocument(package);
        var bytes = Encoding.UTF8.GetBytes(exportDocument);
        await using var stream = new MemoryStream(bytes, writable: false);
        var writeResult = await blobs.PutAsync(
            stream,
            "application/json",
            cancellationToken);

        try
        {
            package.MarkExported(
                writeResult.Sha256,
                writeResult.Length,
                clock.GetUtcNow());
        }
        catch (InvalidOperationException)
        {
            return ExportPublicationPackageResult.InvalidState(
                "PUBLICATION_PACKAGE_INVALID_STATE");
        }

        await changes.CommitAsync(cancellationToken);
        return ExportPublicationPackageResult.Exported(package);
    }

    private static string BuildExportDocument(PublicationPackage package)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Content OS publication export");
        builder.AppendLine();
        builder.AppendLine($"packageId: {package.Id:D}");
        builder.AppendLine($"editionId: {package.EditionId:D}");
        builder.AppendLine($"rendererVersion: {package.RendererVersion}");
        builder.AppendLine();
        builder.AppendLine("## Manifest");
        builder.AppendLine();
        builder.AppendLine("```json");
        builder.AppendLine(package.ManifestJson);
        builder.AppendLine("```");
        return builder.ToString();
    }
}
