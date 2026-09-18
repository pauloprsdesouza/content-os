namespace ContentOS.Contracts.Content;

public sealed record StartTopicDiscoveryRequest(
    string Format,
    string AreaId,
    string AreaName,
    bool AreaIsSubfield,
    int WindowDays);
