namespace ContentOS.Domain.Learning;

public sealed class Capstone
{
    private Capstone(
        Guid id,
        Guid enrollmentId,
        string title,
        DateTimeOffset createdAt)
    {
        Id = id;
        EnrollmentId = enrollmentId;
        Title = title;
        Status = CapstoneStatus.Open;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private Capstone()
    {
        Title = null!;
    }

    public Guid Id { get; private set; }

    public Guid EnrollmentId { get; private set; }

    public string Title { get; private set; }

    public CapstoneStatus Status { get; private set; }

    public DateTimeOffset? SubmittedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Capstone Create(
        Guid id,
        Guid enrollmentId,
        string title,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Capstone id is required.", nameof(id));
        }

        if (enrollmentId == Guid.Empty)
        {
            throw new ArgumentException("Enrollment id is required.", nameof(enrollmentId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        return new Capstone(id, enrollmentId, title.Trim(), createdAt);
    }

    public void Submit(DateTimeOffset submittedAt)
    {
        if (Status != CapstoneStatus.Open)
        {
            throw new InvalidOperationException(
                $"Capstone {Id} cannot submit from {Status}.");
        }

        Status = CapstoneStatus.Submitted;
        SubmittedAt = submittedAt;
        UpdatedAt = submittedAt;
    }

    public void MarkEvaluated(DateTimeOffset evaluatedAt)
    {
        if (Status != CapstoneStatus.Submitted)
        {
            throw new InvalidOperationException(
                $"Capstone {Id} cannot evaluate from {Status}.");
        }

        Status = CapstoneStatus.Evaluated;
        UpdatedAt = evaluatedAt;
    }
}
