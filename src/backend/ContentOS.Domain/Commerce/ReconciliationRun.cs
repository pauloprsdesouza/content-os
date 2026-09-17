namespace ContentOS.Domain.Commerce;

public sealed class ReconciliationRun
{
    private ReconciliationRun(Guid id, DateTimeOffset startedAt)
    {
        Id = id;
        Status = ReconciliationRunStatus.Running;
        StartedAt = startedAt;
        ProcessedCount = 0;
        ConfirmedCount = 0;
        IgnoredCount = 0;
    }

    private ReconciliationRun()
    {
    }

    public Guid Id { get; private set; }

    public ReconciliationRunStatus Status { get; private set; }

    public DateTimeOffset StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public int ProcessedCount { get; private set; }

    public int ConfirmedCount { get; private set; }

    public int IgnoredCount { get; private set; }

    public string? Notes { get; private set; }

    public static ReconciliationRun Start(Guid id, DateTimeOffset startedAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Reconciliation run id is required.", nameof(id));
        }

        return new ReconciliationRun(id, startedAt);
    }

    public void RecordConfirmed()
    {
        EnsureRunning();
        ProcessedCount++;
        ConfirmedCount++;
    }

    public void RecordIgnored()
    {
        EnsureRunning();
        ProcessedCount++;
        IgnoredCount++;
    }

    public void Complete(DateTimeOffset completedAt, string? notes = null)
    {
        EnsureRunning();
        Status = ReconciliationRunStatus.Completed;
        CompletedAt = completedAt;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void Fail(DateTimeOffset failedAt, string notes)
    {
        EnsureRunning();
        ArgumentException.ThrowIfNullOrWhiteSpace(notes);
        Status = ReconciliationRunStatus.Failed;
        CompletedAt = failedAt;
        Notes = notes.Trim();
    }

    private void EnsureRunning()
    {
        if (Status != ReconciliationRunStatus.Running)
        {
            throw new InvalidOperationException(
                $"Reconciliation run {Id} is not running.");
        }
    }
}
