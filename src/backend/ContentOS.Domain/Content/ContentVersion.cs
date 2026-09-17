namespace ContentOS.Domain.Content;

public sealed class ContentVersion
{
    private ContentVersion(
        Guid id,
        Guid contentUnitId,
        int revision,
        string? bodyMarkdown,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        Id = id;
        ContentUnitId = contentUnitId;
        Revision = revision;
        BodyMarkdown = bodyMarkdown;
        Status = ContentVersionStatus.Draft;
        Version = 1;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private ContentVersion()
    {
    }

    public Guid Id { get; private set; }

    public Guid ContentUnitId { get; private set; }

    public int Revision { get; private set; }

    public string? BodyMarkdown { get; private set; }

    public ContentVersionStatus Status { get; private set; }

    public long Version { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public Guid? ReviewedByUserId { get; private set; }

    public DateTimeOffset? ReviewedAt { get; private set; }

    public string? ChangeRequestNotes { get; private set; }

    public string? AgentReviewNotes { get; private set; }

    public Guid? OperationId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static ContentVersion CreateDraft(
        Guid id,
        Guid contentUnitId,
        int revision,
        string? bodyMarkdown,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Content version id is required.", nameof(id));
        }

        if (contentUnitId == Guid.Empty)
        {
            throw new ArgumentException("Content unit id is required.", nameof(contentUnitId));
        }

        if (revision < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(revision), "Revision must be >= 1.");
        }

        if (createdByUserId == Guid.Empty)
        {
            throw new ArgumentException("Creator id is required.", nameof(createdByUserId));
        }

        return new ContentVersion(
            id,
            contentUnitId,
            revision,
            NormalizeBody(bodyMarkdown),
            createdByUserId,
            createdAt);
    }

    public void BindOperation(Guid operationId, DateTimeOffset at)
    {
        if (operationId == Guid.Empty)
        {
            throw new ArgumentException("Operation id is required.", nameof(operationId));
        }

        OperationId = operationId;
        Touch(at);
    }

    public void UpdateDraftBody(string bodyMarkdown, long expectedVersion, DateTimeOffset at)
    {
        EnsureExpectedVersion(expectedVersion);
        EnsureMutableDraft();

        BodyMarkdown = NormalizeBody(bodyMarkdown)
            ?? throw new ArgumentException("Body is required.", nameof(bodyMarkdown));
        Version++;
        Touch(at);
    }

    public void QueueGeneration(DateTimeOffset at)
    {
        if (Status is not (ContentVersionStatus.Draft or ContentVersionStatus.ChangesRequested))
        {
            throw new InvalidOperationException(
                $"Content version {Id} cannot queue generation from {Status}.");
        }

        Status = ContentVersionStatus.GenerationQueued;
        ChangeRequestNotes = null;
        Version++;
        Touch(at);
    }

    public void MarkGenerated(string bodyMarkdown, DateTimeOffset at)
    {
        if (Status != ContentVersionStatus.GenerationQueued)
        {
            throw new InvalidOperationException(
                $"Content version {Id} cannot mark generated from {Status}.");
        }

        BodyMarkdown = NormalizeBody(bodyMarkdown)
            ?? throw new ArgumentException("Generated body is required.", nameof(bodyMarkdown));
        Status = ContentVersionStatus.Generated;
        Version++;
        Touch(at);
    }

    public void QueueAgentReview(DateTimeOffset at)
    {
        if (Status != ContentVersionStatus.Generated)
        {
            throw new InvalidOperationException(
                $"Content version {Id} cannot queue agent review from {Status}.");
        }

        Status = ContentVersionStatus.ReviewQueued;
        Version++;
        Touch(at);
    }

    public void MarkAgentReviewed(string? notes, DateTimeOffset at)
    {
        if (Status != ContentVersionStatus.ReviewQueued)
        {
            throw new InvalidOperationException(
                $"Content version {Id} cannot mark agent-reviewed from {Status}.");
        }

        AgentReviewNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        Status = ContentVersionStatus.AgentReviewed;
        Version++;
        Touch(at);
    }

    public void RequestHumanApproval(DateTimeOffset at)
    {
        if (Status is not (
            ContentVersionStatus.AgentReviewed
            or ContentVersionStatus.Generated
            or ContentVersionStatus.Draft))
        {
            throw new InvalidOperationException(
                $"Content version {Id} cannot request human approval from {Status}.");
        }

        if (string.IsNullOrWhiteSpace(BodyMarkdown))
        {
            throw new InvalidOperationException(
                $"Content version {Id} requires body before human approval.");
        }

        Status = ContentVersionStatus.PendingHumanApproval;
        Version++;
        Touch(at);
    }

    public void Approve(
        Guid actorUserId,
        long expectedVersion,
        DateTimeOffset approvedAt)
    {
        if (actorUserId == Guid.Empty)
        {
            throw new ArgumentException("Reviewer id is required.", nameof(actorUserId));
        }

        EnsureExpectedVersion(expectedVersion);

        if (Status != ContentVersionStatus.PendingHumanApproval)
        {
            throw new InvalidOperationException(
                $"Content version {Id} cannot be approved from {Status}.");
        }

        Status = ContentVersionStatus.Approved;
        ReviewedByUserId = actorUserId;
        ReviewedAt = approvedAt;
        ChangeRequestNotes = null;
        Version++;
        Touch(approvedAt);
    }

    public void RequestChanges(
        Guid actorUserId,
        string notes,
        long expectedVersion,
        DateTimeOffset at)
    {
        if (actorUserId == Guid.Empty)
        {
            throw new ArgumentException("Reviewer id is required.", nameof(actorUserId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(notes);
        EnsureExpectedVersion(expectedVersion);

        if (Status != ContentVersionStatus.PendingHumanApproval)
        {
            throw new InvalidOperationException(
                $"Content version {Id} cannot request changes from {Status}.");
        }

        Status = ContentVersionStatus.ChangesRequested;
        ReviewedByUserId = actorUserId;
        ReviewedAt = at;
        ChangeRequestNotes = notes.Trim();
        Version++;
        Touch(at);
    }

    private void EnsureMutableDraft()
    {
        if (Status is not (ContentVersionStatus.Draft or ContentVersionStatus.ChangesRequested))
        {
            throw new InvalidOperationException(
                $"Content version {Id} body cannot change while status is {Status}.");
        }
    }

    private void EnsureExpectedVersion(long expectedVersion)
    {
        if (expectedVersion != Version)
        {
            throw new InvalidOperationException(
                $"Content version {Id} version mismatch. Expected {expectedVersion}, actual {Version}.");
        }
    }

    private static string? NormalizeBody(string? body) =>
        string.IsNullOrWhiteSpace(body) ? null : body.Trim();

    private void Touch(DateTimeOffset at) => UpdatedAt = at;
}
