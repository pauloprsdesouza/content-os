namespace ContentOS.Domain.Research;

public sealed class ResearchFinding
{
    private ResearchFinding(
        Guid id,
        Guid researchJobId,
        string statement,
        decimal confidence,
        DateTimeOffset createdAt)
    {
        Id = id;
        ResearchJobId = researchJobId;
        Statement = statement;
        Confidence = confidence;
        CreatedAt = createdAt;
    }

    private ResearchFinding()
    {
        Statement = null!;
    }

    public Guid Id { get; private set; }

    public Guid ResearchJobId { get; private set; }

    public string Statement { get; private set; }

    public decimal Confidence { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static ResearchFinding Create(
        Guid id,
        Guid researchJobId,
        string statement,
        decimal confidence,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Finding id is required.", nameof(id));
        }

        if (researchJobId == Guid.Empty)
        {
            throw new ArgumentException("Research job id is required.", nameof(researchJobId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(statement);

        if (confidence is < 0m or > 1m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(confidence),
                "Confidence must be between 0 and 1.");
        }

        return new ResearchFinding(
            id,
            researchJobId,
            statement.Trim(),
            confidence,
            createdAt);
    }
}
