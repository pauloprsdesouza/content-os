namespace ContentOS.Domain.Publication;

public sealed class PublicationPackage
{
    private PublicationPackage(
        Guid id,
        Guid editionId,
        DateTimeOffset createdAt)
    {
        Id = id;
        EditionId = editionId;
        Status = PublicationPackageStatus.Requested;
        RendererVersion = PublicationRendererVersion.MvpV1;
        Version = 1;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private PublicationPackage()
    {
        RendererVersion = null!;
    }

    public Guid Id { get; private set; }

    public Guid EditionId { get; private set; }

    public PublicationPackageStatus Status { get; private set; }

    public string RendererVersion { get; private set; }

    public string? ManifestJson { get; private set; }

    public string? ExportBlobSha256 { get; private set; }

    public long? ExportBlobLength { get; private set; }

    public Guid? ConfirmedByUserId { get; private set; }

    public DateTimeOffset? ConfirmedAt { get; private set; }

    public long Version { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static PublicationPackage CreateRequested(
        Guid id,
        Guid editionId,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Package id is required.", nameof(id));
        }

        if (editionId == Guid.Empty)
        {
            throw new ArgumentException("Edition id is required.", nameof(editionId));
        }

        return new PublicationPackage(id, editionId, createdAt);
    }

    public void StartBuilding(DateTimeOffset at)
    {
        EnsureStatus(PublicationPackageStatus.Requested, "start building");
        Status = PublicationPackageStatus.Building;
        Version++;
        Touch(at);
    }

    public void MarkValidating(PublicationPackageManifest manifest, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        EnsureStatus(PublicationPackageStatus.Building, "mark validating");

        if (!string.Equals(
                manifest.RendererVersion,
                RendererVersion,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Package {Id} renderer version mismatch.");
        }

        ManifestJson = manifest.ToCanonicalJson();
        Status = PublicationPackageStatus.Validating;
        Version++;
        Touch(at);
    }

    public void MarkReadyForExport(DateTimeOffset at)
    {
        EnsureStatus(PublicationPackageStatus.Validating, "mark ready for export");

        if (string.IsNullOrWhiteSpace(ManifestJson))
        {
            throw new InvalidOperationException(
                $"Package {Id} requires a manifest before ReadyForExport.");
        }

        Status = PublicationPackageStatus.ReadyForExport;
        Version++;
        Touch(at);
    }

    public void MarkExported(
        string exportBlobSha256,
        long exportBlobLength,
        DateTimeOffset at)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exportBlobSha256);
        if (exportBlobLength < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(exportBlobLength),
                "Export blob length must be >= 0.");
        }

        EnsureStatus(PublicationPackageStatus.ReadyForExport, "mark exported");

        ExportBlobSha256 = exportBlobSha256.Trim().ToLowerInvariant();
        ExportBlobLength = exportBlobLength;
        Status = PublicationPackageStatus.Exported;
        Version++;
        Touch(at);
    }

    public void ConfirmPublication(
        Guid actorUserId,
        long expectedVersion,
        DateTimeOffset confirmedAt)
    {
        if (actorUserId == Guid.Empty)
        {
            throw new ArgumentException("Actor id is required.", nameof(actorUserId));
        }

        EnsureExpectedVersion(expectedVersion);
        EnsureStatus(PublicationPackageStatus.Exported, "confirm publication");

        Status = PublicationPackageStatus.PublishedConfirmed;
        ConfirmedByUserId = actorUserId;
        ConfirmedAt = confirmedAt;
        Version++;
        Touch(confirmedAt);
    }

    private void EnsureStatus(PublicationPackageStatus expected, string action)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException(
                $"Package {Id} cannot {action} from {Status}.");
        }
    }

    private void EnsureExpectedVersion(long expectedVersion)
    {
        if (expectedVersion != Version)
        {
            throw new InvalidOperationException(
                $"Package {Id} version mismatch. Expected {expectedVersion}, actual {Version}.");
        }
    }

    private void Touch(DateTimeOffset at) => UpdatedAt = at;
}
