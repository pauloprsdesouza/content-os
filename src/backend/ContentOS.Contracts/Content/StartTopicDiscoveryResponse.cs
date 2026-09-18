namespace ContentOS.Contracts.Content;

public sealed record StartTopicDiscoveryResponse(Guid DiscoveryId, Guid? OperationId, bool IsEmpty);
