namespace ContentOS.Domain.Learning;

public sealed class Outcome
{
    private Outcome(
        Guid id,
        Guid learnerId,
        Guid enrollmentId,
        Guid? capstoneId,
        bool passed,
        DateTimeOffset recordedAt)
    {
        Id = id;
        LearnerId = learnerId;
        EnrollmentId = enrollmentId;
        CapstoneId = capstoneId;
        Passed = passed;
        RecordedAt = recordedAt;
    }

    private Outcome()
    {
    }

    public Guid Id { get; private set; }

    public Guid LearnerId { get; private set; }

    public Guid EnrollmentId { get; private set; }

    public Guid? CapstoneId { get; private set; }

    public bool Passed { get; private set; }

    public DateTimeOffset RecordedAt { get; private set; }

    public static Outcome Record(
        Guid id,
        Guid learnerId,
        Guid enrollmentId,
        Guid? capstoneId,
        bool passed,
        DateTimeOffset recordedAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Outcome id is required.", nameof(id));
        }

        if (learnerId == Guid.Empty)
        {
            throw new ArgumentException("Learner id is required.", nameof(learnerId));
        }

        if (enrollmentId == Guid.Empty)
        {
            throw new ArgumentException("Enrollment id is required.", nameof(enrollmentId));
        }

        return new Outcome(
            id,
            learnerId,
            enrollmentId,
            capstoneId == Guid.Empty ? null : capstoneId,
            passed,
            recordedAt);
    }
}
