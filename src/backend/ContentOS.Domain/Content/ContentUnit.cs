namespace ContentOS.Domain.Content;

public sealed class ContentUnit
{
    private ContentUnit(
        Guid id,
        string title,
        string? brief,
        ContentFormat format,
        string? citationContentHashes,
        Guid? topicDiscoveryId,
        Guid? productId,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        Id = id;
        Title = title;
        Brief = brief;
        Format = format;
        CitationContentHashes = citationContentHashes;
        TopicDiscoveryId = topicDiscoveryId;
        ProductId = productId;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private ContentUnit()
    {
        Title = null!;
        Format = ContentFormat.Article;
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; }

    public string? Brief { get; private set; }

    public ContentFormat Format { get; private set; }

    public string? CitationContentHashes { get; private set; }

    public Guid? TopicDiscoveryId { get; private set; }

    public Guid? ProductId { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyList<string> ListCitationHashes() =>
        string.IsNullOrWhiteSpace(CitationContentHashes)
            ? []
            : CitationContentHashes.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public static ContentUnit Create(
        Guid id,
        string title,
        string? brief,
        ContentFormat format,
        IReadOnlyList<string>? citationContentHashes,
        Guid? topicDiscoveryId,
        Guid? productId,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Content unit id is required.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(format);

        if (createdByUserId == Guid.Empty)
        {
            throw new ArgumentException("Creator id is required.", nameof(createdByUserId));
        }

        var hashes = citationContentHashes?
            .Where(hash => !string.IsNullOrWhiteSpace(hash))
            .Select(hash => hash.Trim().ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToArray() ?? [];
        if (hashes.Any(hash => hash.Contains("://", StringComparison.Ordinal) || hash.Length != 64))
        {
            throw new ArgumentException("Citation hashes must be content hashes, not URLs.", nameof(citationContentHashes));
        }

        return new ContentUnit(
            id,
            title.Trim(),
            string.IsNullOrWhiteSpace(brief) ? null : brief.Trim(),
            format,
            hashes.Length == 0 ? null : string.Join('\n', hashes),
            topicDiscoveryId,
            productId == Guid.Empty ? null : productId,
            createdByUserId,
            createdAt);
    }

    public void Rename(string title, string? brief, DateTimeOffset at)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        Title = title.Trim();
        Brief = string.IsNullOrWhiteSpace(brief) ? null : brief.Trim();
        UpdatedAt = at;
    }
}
