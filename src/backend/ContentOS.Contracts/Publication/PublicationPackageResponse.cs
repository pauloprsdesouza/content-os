namespace ContentOS.Contracts.Publication;

public sealed record PublicationPackageResponse(
    Guid Id,
    Guid EditionId,
    string Status,
    string RendererVersion,
    string? ManifestJson,
    string? ExportBlobSha256,
    long? ExportBlobLength,
    Guid? ConfirmedByUserId,
    DateTimeOffset? ConfirmedAt,
    long Version,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
