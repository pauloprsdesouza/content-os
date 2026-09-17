using System.Security.Cryptography;
using System.Text;

namespace ContentOS.Domain.Publication;

public sealed class PublicationPackageManifest
{
    private PublicationPackageManifest(
        string rendererVersion,
        IReadOnlyList<Guid> sourceSnapshotIds,
        IReadOnlyList<ManifestContentEntry> contentEntries,
        IReadOnlyList<ManifestAssetEntry> assetEntries)
    {
        RendererVersion = rendererVersion;
        SourceSnapshotIds = sourceSnapshotIds;
        ContentEntries = contentEntries;
        AssetEntries = assetEntries;
    }

    public string RendererVersion { get; }

    public IReadOnlyList<Guid> SourceSnapshotIds { get; }

    public IReadOnlyList<ManifestContentEntry> ContentEntries { get; }

    public IReadOnlyList<ManifestAssetEntry> AssetEntries { get; }

    public static PublicationPackageManifest Create(
        string rendererVersion,
        IReadOnlyList<Guid> sourceSnapshotIds,
        IReadOnlyList<ManifestContentEntry> contentEntries,
        IReadOnlyList<ManifestAssetEntry>? assetEntries = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rendererVersion);
        ArgumentNullException.ThrowIfNull(sourceSnapshotIds);
        ArgumentNullException.ThrowIfNull(contentEntries);

        if (contentEntries.Count == 0)
        {
            throw new ArgumentException(
                "Manifest requires at least one content entry.",
                nameof(contentEntries));
        }

        return new PublicationPackageManifest(
            rendererVersion.Trim(),
            sourceSnapshotIds.ToArray(),
            contentEntries.ToArray(),
            (assetEntries ?? []).ToArray());
    }

    public static string HashContentBody(string? bodyMarkdown)
    {
        var bytes = Encoding.UTF8.GetBytes(bodyMarkdown ?? string.Empty);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public string ToCanonicalJson()
    {
        var builder = new StringBuilder();
        builder.Append('{');
        builder.Append("\"rendererVersion\":");
        AppendJsonString(builder, RendererVersion);
        builder.Append(",\"sourceSnapshotIds\":[");
        for (var index = 0; index < SourceSnapshotIds.Count; index++)
        {
            if (index > 0)
            {
                builder.Append(',');
            }

            AppendJsonString(builder, SourceSnapshotIds[index].ToString("D"));
        }

        builder.Append("],\"contentEntries\":[");
        for (var index = 0; index < ContentEntries.Count; index++)
        {
            if (index > 0)
            {
                builder.Append(',');
            }

            var entry = ContentEntries[index];
            builder.Append("{\"contentVersionId\":");
            AppendJsonString(builder, entry.ContentVersionId.ToString("D"));
            builder.Append(",\"contentSha256\":");
            AppendJsonString(builder, entry.ContentSha256);
            builder.Append('}');
        }

        builder.Append("],\"assetEntries\":[");
        for (var index = 0; index < AssetEntries.Count; index++)
        {
            if (index > 0)
            {
                builder.Append(',');
            }

            var entry = AssetEntries[index];
            builder.Append("{\"assetKey\":");
            AppendJsonString(builder, entry.AssetKey);
            builder.Append(",\"sha256\":");
            AppendJsonString(builder, entry.Sha256);
            builder.Append('}');
        }

        builder.Append("]}");
        return builder.ToString();
    }

    private static void AppendJsonString(StringBuilder builder, string value)
    {
        builder.Append('"');
        foreach (var character in value)
        {
            switch (character)
            {
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '"':
                    builder.Append("\\\"");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                default:
                    builder.Append(character);
                    break;
            }
        }

        builder.Append('"');
    }
}
