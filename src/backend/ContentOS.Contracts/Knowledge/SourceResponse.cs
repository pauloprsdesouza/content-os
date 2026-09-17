namespace ContentOS.Contracts.Knowledge;

public sealed record SourceResponse(
    Guid Id,
    string DisplayName,
    string CanonicalUri,
    string Kind,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
