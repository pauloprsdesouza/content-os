namespace ContentOS.Domain.Learning;

public sealed class Evaluation
{
    private Evaluation(
        Guid id,
        Guid capstoneId,
        bool passed,
        int? score,
        DateTimeOffset evaluatedAt)
    {
        Id = id;
        CapstoneId = capstoneId;
        Passed = passed;
        Score = score;
        EvaluatedAt = evaluatedAt;
    }

    private Evaluation()
    {
    }

    public Guid Id { get; private set; }

    public Guid CapstoneId { get; private set; }

    public bool Passed { get; private set; }

    public int? Score { get; private set; }

    public DateTimeOffset EvaluatedAt { get; private set; }

    public static Evaluation Create(
        Guid id,
        Guid capstoneId,
        bool passed,
        int? score,
        DateTimeOffset evaluatedAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Evaluation id is required.", nameof(id));
        }

        if (capstoneId == Guid.Empty)
        {
            throw new ArgumentException("Capstone id is required.", nameof(capstoneId));
        }

        if (score is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(score), "Score must be 0–100.");
        }

        return new Evaluation(id, capstoneId, passed, score, evaluatedAt);
    }
}
