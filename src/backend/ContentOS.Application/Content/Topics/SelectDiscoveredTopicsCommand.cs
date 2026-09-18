namespace ContentOS.Application.Content.Topics;

public sealed record SelectDiscoveredTopicsCommand(
    Guid DiscoveryId,
    IReadOnlyList<Guid> ProposalIds,
    Guid OwnerUserId,
    Guid? ProductId);
