namespace ContentOS.Contracts.Content;

public sealed record TopicDiscoveryResponse(
    Guid Id,
    string Format,
    string AreaId,
    string AreaName,
    bool AreaIsSubfield,
    int WindowDays,
    string Status,
    Guid? OperationId,
    Guid? ContentUnitId,
    Guid? SeriesId,
    IReadOnlyList<TopicProposalResponse> Proposals);
