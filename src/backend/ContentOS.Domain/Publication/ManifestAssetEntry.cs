namespace ContentOS.Domain.Publication;

public sealed class ManifestAssetEntry
{
    public ManifestAssetEntry(string assetKey, string sha256)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assetKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(sha256);

        AssetKey = assetKey.Trim();
        Sha256 = sha256.Trim().ToLowerInvariant();
    }

    public string AssetKey { get; }

    public string Sha256 { get; }
}
