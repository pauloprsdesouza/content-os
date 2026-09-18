namespace ContentOS.Application.Content.Topics;

public sealed record ApplyTopicLabelsCommand(
    Guid DiscoveryId,
    Guid OperationId,
    bool Succeeded,
    string? ErrorMessage,
    IReadOnlyList<TopicLabelInput> Topics);
