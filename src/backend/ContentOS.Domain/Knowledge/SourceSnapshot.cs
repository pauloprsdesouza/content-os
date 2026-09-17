namespace ContentOS.Domain.Knowledge;

public sealed class SourceSnapshot
{
    private SourceSnapshot(
        Guid id,
        Guid sourceId,
        ContentHash contentHash,
        string mediaType,
        long byteLength,
        DateTimeOffset capturedAt)
    {
        Id = id;
        SourceId = sourceId;
        ContentHash = contentHash;
        MediaType = mediaType;
        ByteLength = byteLength;
        CapturedAt = capturedAt;
    }

    private SourceSnapshot()
    {
        MediaType = null!;
    }

    public Guid Id { get; private set; }

    public Guid SourceId { get; private set; }

    public ContentHash ContentHash { get; private set; }

    public string MediaType { get; private set; }

    public long ByteLength { get; private set; }

    public DateTimeOffset CapturedAt { get; private set; }

    public static SourceSnapshot Create(
        Guid id,
        Guid sourceId,
        ContentHash contentHash,
        string mediaType,
        long byteLength,
        DateTimeOffset capturedAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Snapshot id is required.", nameof(id));
        }

        if (sourceId == Guid.Empty)
        {
            throw new ArgumentException("Source id is required.", nameof(sourceId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(mediaType);

        if (byteLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteLength));
        }

        return new SourceSnapshot(
            id,
            sourceId,
            contentHash,
            mediaType.Trim(),
            byteLength,
            capturedAt);
    }
}
