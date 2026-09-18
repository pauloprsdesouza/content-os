namespace ContentOS.Application.Content.Literature;

public sealed record ScholarlyWork(
    string WorkId,
    string Title,
    int? PublicationYear,
    string? TopicId,
    string? TopicName,
    string AbstractText);
