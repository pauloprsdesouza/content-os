namespace ContentOS.Contracts.Knowledge;

public sealed record SourceListItemResponse(
    Guid Id,
    string DisplayName,
    string CanonicalUri,
    string Kind,
    DateTimeOffset UpdatedAt);
