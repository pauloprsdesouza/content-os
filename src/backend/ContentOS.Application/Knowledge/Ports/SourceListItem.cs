using ContentOS.Domain.Knowledge;

namespace ContentOS.Application.Knowledge.Ports;

public sealed record SourceListItem(
    Guid Id,
    string DisplayName,
    string CanonicalUri,
    SourceKind Kind,
    DateTimeOffset UpdatedAt);
