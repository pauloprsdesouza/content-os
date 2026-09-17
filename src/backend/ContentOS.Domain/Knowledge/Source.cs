namespace ContentOS.Domain.Knowledge;

public sealed class Source
{
    private Source(
        Guid id,
        SourceUri canonicalUri,
        SourceKind kind,
        string displayName,
        DateTimeOffset createdAt)
    {
        Id = id;
        CanonicalUri = canonicalUri;
        Kind = kind;
        DisplayName = displayName;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private Source()
    {
        DisplayName = null!;
    }

    public Guid Id { get; private set; }

    public SourceUri CanonicalUri { get; private set; }

    public SourceKind Kind { get; private set; }

    public string DisplayName { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Source Create(
        Guid id,
        SourceUri canonicalUri,
        SourceKind kind,
        string displayName,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Source id is required.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        return new Source(
            id,
            canonicalUri,
            kind,
            displayName.Trim(),
            createdAt);
    }
}
