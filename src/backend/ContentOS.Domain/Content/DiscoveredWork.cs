namespace ContentOS.Domain.Content;

public sealed class DiscoveredWork
{
    private DiscoveredWork(
        Guid id,
        Guid discoveryId,
        string workId,
        string title,
        int? publicationYear,
        string? topicId,
        string? topicName,
        string abstractText)
    {
        Id = id;
        DiscoveryId = discoveryId;
        WorkId = workId;
        Title = title;
        PublicationYear = publicationYear;
        TopicId = topicId;
        TopicName = topicName;
        AbstractText = abstractText;
    }

    private DiscoveredWork()
    {
        WorkId = null!;
        Title = null!;
        AbstractText = null!;
    }

    public Guid Id { get; private set; }

    public Guid DiscoveryId { get; private set; }

    public string WorkId { get; private set; }

    public string Title { get; private set; }

    public int? PublicationYear { get; private set; }

    public string? TopicId { get; private set; }

    public string? TopicName { get; private set; }

    public string AbstractText { get; private set; }

    public static DiscoveredWork Create(
        Guid id,
        Guid discoveryId,
        string workId,
        string title,
        int? publicationYear,
        string? topicId,
        string? topicName,
        string abstractText)
    {
        if (id == Guid.Empty || discoveryId == Guid.Empty)
        {
            throw new ArgumentException("Work and discovery ids are required.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(workId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        if (workId.Contains("://", StringComparison.Ordinal) || workId.Contains('/', StringComparison.Ordinal))
        {
            throw new ArgumentException("Work id must be an identifier, not a URL.", nameof(workId));
        }

        return new DiscoveredWork(
            id,
            discoveryId,
            workId.Trim(),
            title.Trim(),
            publicationYear,
            string.IsNullOrWhiteSpace(topicId) ? null : topicId.Trim(),
            string.IsNullOrWhiteSpace(topicName) ? null : topicName.Trim(),
            abstractText?.Trim() ?? string.Empty);
    }
}
