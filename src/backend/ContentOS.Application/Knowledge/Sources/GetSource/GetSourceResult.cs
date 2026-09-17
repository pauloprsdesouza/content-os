using ContentOS.Domain.Knowledge;

namespace ContentOS.Application.Knowledge.Sources.GetSource;

public sealed record GetSourceResult
{
    private GetSourceResult(
        bool found,
        Guid? id,
        string? displayName,
        string? canonicalUri,
        SourceKind? kind,
        DateTimeOffset? createdAt,
        DateTimeOffset? updatedAt)
    {
        Found = found;
        Id = id;
        DisplayName = displayName;
        CanonicalUri = canonicalUri;
        Kind = kind;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public bool Found { get; }

    public Guid? Id { get; }

    public string? DisplayName { get; }

    public string? CanonicalUri { get; }

    public SourceKind? Kind { get; }

    public DateTimeOffset? CreatedAt { get; }

    public DateTimeOffset? UpdatedAt { get; }

    public static GetSourceResult NotFound() =>
        new(false, null, null, null, null, null, null);

    public static GetSourceResult From(Source source) =>
        new(
            true,
            source.Id,
            source.DisplayName,
            source.CanonicalUri.ToString(),
            source.Kind,
            source.CreatedAt,
            source.UpdatedAt);
}
