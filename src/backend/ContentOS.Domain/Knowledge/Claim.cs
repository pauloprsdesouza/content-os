namespace ContentOS.Domain.Knowledge;

public sealed class Claim
{
    private readonly List<ClaimEvidenceLink> _evidenceLinks = [];

    private Claim(
        Guid id,
        string statement,
        ConfidenceScore confidence,
        DateTimeOffset createdAt)
    {
        Id = id;
        Statement = statement;
        Confidence = confidence;
        Status = ClaimStatus.Draft;
        Version = 1;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private Claim()
    {
        Statement = null!;
    }

    public Guid Id { get; private set; }

    public string Statement { get; private set; }

    public ClaimStatus Status { get; private set; }

    public ConfidenceScore Confidence { get; private set; }

    public long Version { get; private set; }

    public Guid? ReviewedByUserId { get; private set; }

    public DateTimeOffset? ReviewedAt { get; private set; }

    public string? RejectionReason { get; private set; }

    public Guid? SupersededByClaimId { get; private set; }

    public Guid? OriginResearchFindingId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<ClaimEvidenceLink> EvidenceLinks => _evidenceLinks;

    public static Claim Create(
        Guid id,
        string statement,
        ConfidenceScore confidence,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Claim id is required.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(statement);

        return new Claim(id, statement.Trim(), confidence, createdAt);
    }

    public void AttachOrigin(Guid researchFindingId)
    {
        if (researchFindingId == Guid.Empty)
        {
            throw new ArgumentException("Research finding id is required.", nameof(researchFindingId));
        }

        if (OriginResearchFindingId is not null && OriginResearchFindingId != researchFindingId)
        {
            throw new InvalidOperationException("Claim origin is already set.");
        }

        OriginResearchFindingId = researchFindingId;
    }

    public void LinkEvidence(Guid evidenceId, DateTimeOffset linkedAt)
    {
        EnsureMutableReviewState();

        if (_evidenceLinks.Any(link => link.EvidenceId == evidenceId))
        {
            return;
        }

        _evidenceLinks.Add(ClaimEvidenceLink.Create(Id, evidenceId, linkedAt));
        Touch(linkedAt);
    }

    public void SubmitForReview(DateTimeOffset submittedAt)
    {
        if (Status != ClaimStatus.Draft)
        {
            throw new InvalidOperationException(
                $"Claim {Id} cannot move to PendingReview from {Status}.");
        }

        if (_evidenceLinks.Count == 0)
        {
            throw new InvalidOperationException(
                $"Claim {Id} requires evidence before review.");
        }

        Status = ClaimStatus.PendingReview;
        Touch(submittedAt);
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

        if (Status != ClaimStatus.PendingReview)
        {
            throw new InvalidOperationException(
                $"Claim {Id} cannot be approved from {Status}.");
        }

        if (_evidenceLinks.Count == 0)
        {
            throw new InvalidOperationException(
                $"Claim {Id} cannot be approved without evidence.");
        }

        Status = ClaimStatus.Approved;
        ReviewedByUserId = actorUserId;
        ReviewedAt = approvedAt;
        RejectionReason = null;
        Version++;
        Touch(approvedAt);
    }

    public void Reject(
        Guid actorUserId,
        string reason,
        long expectedVersion,
        DateTimeOffset rejectedAt)
    {
        if (actorUserId == Guid.Empty)
        {
            throw new ArgumentException("Reviewer id is required.", nameof(actorUserId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        EnsureExpectedVersion(expectedVersion);

        if (Status != ClaimStatus.PendingReview)
        {
            throw new InvalidOperationException(
                $"Claim {Id} cannot be rejected from {Status}.");
        }

        Status = ClaimStatus.Rejected;
        ReviewedByUserId = actorUserId;
        ReviewedAt = rejectedAt;
        RejectionReason = reason.Trim();
        Version++;
        Touch(rejectedAt);
    }

    public void MarkSuperseded(Guid successorClaimId, DateTimeOffset supersededAt)
    {
        if (successorClaimId == Guid.Empty)
        {
            throw new ArgumentException("Successor claim id is required.", nameof(successorClaimId));
        }

        if (Status != ClaimStatus.Approved)
        {
            throw new InvalidOperationException(
                $"Only approved claims can be superseded. Claim {Id} is {Status}.");
        }

        Status = ClaimStatus.Superseded;
        SupersededByClaimId = successorClaimId;
        Version++;
        Touch(supersededAt);
    }

    private void EnsureMutableReviewState()
    {
        if (Status is not (ClaimStatus.Draft or ClaimStatus.PendingReview))
        {
            throw new InvalidOperationException(
                $"Claim {Id} evidence cannot change while status is {Status}.");
        }
    }

    private void EnsureExpectedVersion(long expectedVersion)
    {
        if (expectedVersion != Version)
        {
            throw new InvalidOperationException(
                $"Claim {Id} version mismatch. Expected {expectedVersion}, actual {Version}.");
        }
    }

    private void Touch(DateTimeOffset at)
    {
        UpdatedAt = at;
    }
}
