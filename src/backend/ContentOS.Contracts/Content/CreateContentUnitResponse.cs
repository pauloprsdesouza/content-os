namespace ContentOS.Contracts.Content;

public sealed record CreateContentUnitResponse(
    Guid Id,
    Guid VersionId);
