namespace ContentOS.Domain.Knowledge;

public sealed class Evidence
{
    private Evidence(
        Guid id,
        Guid snapshotId,
        EvidenceLocator locator,
        string extractionMethod,
        ConfidenceScore confidence,
        DateTimeOffset createdAt)
    {
        Id = id;
        SnapshotId = snapshotId;
        Locator = locator;
        ExtractionMethod = extractionMethod;
        Confidence = confidence;
        CreatedAt = createdAt;
    }

    private Evidence()
    {
        ExtractionMethod = null!;
    }

    public Guid Id { get; private set; }

    public Guid SnapshotId { get; private set; }

    public EvidenceLocator Locator { get; private set; }

    public string ExtractionMethod { get; private set; }

    public ConfidenceScore Confidence { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static Evidence Create(
        Guid id,
        Guid snapshotId,
        EvidenceLocator locator,
        string extractionMethod,
        ConfidenceScore confidence,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Evidence id is required.", nameof(id));
        }

        if (snapshotId == Guid.Empty)
        {
            throw new ArgumentException("Snapshot id is required.", nameof(snapshotId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(extractionMethod);

        return new Evidence(
            id,
            snapshotId,
            locator,
            extractionMethod.Trim(),
            confidence,
            createdAt);
    }
}
