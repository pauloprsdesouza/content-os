namespace ContentOS.Contracts.Knowledge;

public sealed record CreateSourceRequest(
    string Location,
    string Kind,
    string DisplayName);
