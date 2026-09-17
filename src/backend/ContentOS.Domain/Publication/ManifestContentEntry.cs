namespace ContentOS.Domain.Publication;

public sealed class ManifestContentEntry
{
    public ManifestContentEntry(Guid contentVersionId, string contentSha256)
    {
        if (contentVersionId == Guid.Empty)
        {
            throw new ArgumentException("Content version id is required.", nameof(contentVersionId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(contentSha256);

        ContentVersionId = contentVersionId;
        ContentSha256 = contentSha256.Trim().ToLowerInvariant();
    }

    public Guid ContentVersionId { get; }

    public string ContentSha256 { get; }
}
