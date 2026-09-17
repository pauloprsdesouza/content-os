namespace ContentOS.Domain.Research;

public sealed class ResearchJob
{
    private readonly List<ResearchFinding> _findings = [];

    private ResearchJob(
        Guid id,
        string topic,
        string? scopeNotes,
        Guid requestedByUserId,
        DateTimeOffset createdAt)
    {
        Id = id;
        Topic = topic;
        ScopeNotes = scopeNotes;
        RequestedByUserId = requestedByUserId;
        Status = ResearchJobStatus.Requested;
        Version = 1;
        AttemptCount = 0;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private ResearchJob()
    {
        Topic = null!;
    }

    public Guid Id { get; private set; }

    public string Topic { get; private set; }

    public string? ScopeNotes { get; private set; }

    public Guid RequestedByUserId { get; private set; }

    public ResearchJobStatus Status { get; private set; }

    public long Version { get; private set; }

    public int AttemptCount { get; private set; }

    public string? FailureReason { get; private set; }

    public Guid? OperationId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<ResearchFinding> Findings => _findings;

    public static ResearchJob Create(
        Guid id,
        string topic,
        string? scopeNotes,
        Guid requestedByUserId,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Research job id is required.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(topic);

        if (requestedByUserId == Guid.Empty)
        {
            throw new ArgumentException("Requester id is required.", nameof(requestedByUserId));
        }

        return new ResearchJob(
            id,
            topic.Trim(),
            string.IsNullOrWhiteSpace(scopeNotes) ? null : scopeNotes.Trim(),
            requestedByUserId,
            createdAt);
    }

    public void BindOperation(Guid operationId, DateTimeOffset at)
    {
        if (operationId == Guid.Empty)
        {
            throw new ArgumentException("Operation id is required.", nameof(operationId));
        }

        if (OperationId is not null && OperationId != operationId)
        {
            throw new InvalidOperationException(
                $"Research job {Id} is already bound to operation {OperationId}.");
        }

        OperationId = operationId;
        Touch(at);
    }

    public void MarkQueued(DateTimeOffset at)
    {
        EnsureTransition(ResearchJobStatus.Requested, ResearchJobStatus.Queued);
        Status = ResearchJobStatus.Queued;
        FailureReason = null;
        Version++;
        Touch(at);
    }

    public void MarkRunning(DateTimeOffset at)
    {
        if (Status is not (ResearchJobStatus.Queued or ResearchJobStatus.RetryScheduled))
        {
            throw new InvalidOperationException(
                $"Research job {Id} cannot move to Running from {Status}.");
        }

        Status = ResearchJobStatus.Running;
        AttemptCount++;
        FailureReason = null;
        Version++;
        Touch(at);
    }

    public void MarkAwaitingKnowledgeReview(
        IReadOnlyList<ResearchFinding> findings,
        DateTimeOffset at)
    {
        if (Status != ResearchJobStatus.Running)
        {
            throw new InvalidOperationException(
                $"Research job {Id} cannot await knowledge review from {Status}.");
        }

        ArgumentNullException.ThrowIfNull(findings);
        if (findings.Count == 0)
        {
            throw new InvalidOperationException(
                $"Research job {Id} requires at least one finding before review.");
        }

        _findings.Clear();
        foreach (var finding in findings)
        {
            if (finding.ResearchJobId != Id)
            {
                throw new InvalidOperationException(
                    $"Finding {finding.Id} does not belong to research job {Id}.");
            }

            _findings.Add(finding);
        }

        Status = ResearchJobStatus.AwaitingKnowledgeReview;
        FailureReason = null;
        Version++;
        Touch(at);
    }

    public void MarkCompleted(DateTimeOffset at)
    {
        if (Status != ResearchJobStatus.AwaitingKnowledgeReview)
        {
            throw new InvalidOperationException(
                $"Research job {Id} cannot complete from {Status}.");
        }

        Status = ResearchJobStatus.Completed;
        FailureReason = null;
        Version++;
        Touch(at);
    }

    public void ScheduleRetry(string reason, DateTimeOffset at)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (Status is not (ResearchJobStatus.Running or ResearchJobStatus.Queued))
        {
            throw new InvalidOperationException(
                $"Research job {Id} cannot schedule retry from {Status}.");
        }

        Status = ResearchJobStatus.RetryScheduled;
        FailureReason = reason.Trim();
        Version++;
        Touch(at);
    }

    public void MarkFailed(string reason, DateTimeOffset at)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (Status is ResearchJobStatus.Completed or ResearchJobStatus.Cancelled)
        {
            throw new InvalidOperationException(
                $"Research job {Id} cannot fail from {Status}.");
        }

        Status = ResearchJobStatus.Failed;
        FailureReason = reason.Trim();
        Version++;
        Touch(at);
    }

    public void Cancel(string? reason, DateTimeOffset at)
    {
        if (Status is ResearchJobStatus.Completed or ResearchJobStatus.Cancelled)
        {
            throw new InvalidOperationException(
                $"Research job {Id} cannot be cancelled from {Status}.");
        }

        Status = ResearchJobStatus.Cancelled;
        FailureReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        Version++;
        Touch(at);
    }

    private void EnsureTransition(ResearchJobStatus from, ResearchJobStatus to)
    {
        if (Status != from)
        {
            throw new InvalidOperationException(
                $"Research job {Id} cannot move to {to} from {Status}.");
        }
    }

    private void Touch(DateTimeOffset at) => UpdatedAt = at;
}
