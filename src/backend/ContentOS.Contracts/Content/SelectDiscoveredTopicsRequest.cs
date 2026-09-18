namespace ContentOS.Contracts.Content;

public sealed record SelectDiscoveredTopicsRequest(
    IReadOnlyList<Guid> ProposalIds,
    Guid? ProductId);
