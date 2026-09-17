using ContentOS.Application.Catalog.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Application.Publication.Ports;
using ContentOS.Domain.Publication;
using ContentOS.SharedKernel;

namespace ContentOS.Application.Publication.CreatePackage;

public sealed class CreatePublicationPackageHandler(
    IEditionRepository editions,
    IApprovedContentVersionLookup approvedVersions,
    IPublicationPackageRepository packages,
    IIdGenerator ids,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<CreatePublicationPackageResult> HandleAsync(
        CreatePublicationPackageCommand command,
        CancellationToken cancellationToken = default)
    {
        var edition = await editions.GetByIdAsync(command.EditionId, cancellationToken);
        if (edition is null)
        {
            return CreatePublicationPackageResult.NotFound();
        }

        var contentVersionIds = edition.Curriculum
            .OrderBy(item => item.Position)
            .Select(item => item.ContentVersionId)
            .ToArray();

        if (contentVersionIds.Length == 0)
        {
            return CreatePublicationPackageResult.InvalidCurriculum("CURRICULUM_EMPTY");
        }

        var approved = await approvedVersions.GetApprovedAsync(
            contentVersionIds,
            cancellationToken);

        if (approved.Count != contentVersionIds.Length)
        {
            return CreatePublicationPackageResult.InvalidCurriculum(
                "CURRICULUM_REQUIRES_APPROVED_VERSIONS");
        }

        var contentEntries = new List<ManifestContentEntry>(contentVersionIds.Length);
        foreach (var versionId in contentVersionIds)
        {
            if (!approved.TryGetValue(versionId, out var info))
            {
                return CreatePublicationPackageResult.InvalidCurriculum(
                    "CURRICULUM_REQUIRES_APPROVED_VERSIONS");
            }

            contentEntries.Add(
                new ManifestContentEntry(
                    versionId,
                    PublicationPackageManifest.HashContentBody(info.BodyMarkdown)));
        }

        var now = clock.GetUtcNow();
        var package = PublicationPackage.CreateRequested(
            ids.NewId(),
            edition.Id,
            now);
        var manifest = PublicationPackageManifest.Create(
            PublicationRendererVersion.MvpV1,
            sourceSnapshotIds: [],
            contentEntries,
            assetEntries: []);

        package.StartBuilding(now);
        package.MarkValidating(manifest, now);
        package.MarkReadyForExport(now);

        packages.Add(package);
        await changes.CommitAsync(cancellationToken);
        return CreatePublicationPackageResult.Created(package);
    }
}
