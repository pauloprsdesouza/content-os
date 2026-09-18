namespace ContentOS.Application.Content.Topics;

public sealed record TopicLabelWork(
    string WorkId,
    string Title,
    int? Year,
    string? TopicName,
    string AbstractText);
