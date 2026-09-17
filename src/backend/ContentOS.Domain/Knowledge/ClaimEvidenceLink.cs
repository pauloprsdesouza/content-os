namespace ContentOS.Domain.Knowledge;

public sealed class ClaimEvidenceLink
{
    private ClaimEvidenceLink(Guid claimId, Guid evidenceId, DateTimeOffset linkedAt)
    {
        ClaimId = claimId;
        EvidenceId = evidenceId;
        LinkedAt = linkedAt;
    }

    private ClaimEvidenceLink()
    {
    }

    public Guid ClaimId { get; private set; }

    public Guid EvidenceId { get; private set; }

    public DateTimeOffset LinkedAt { get; private set; }

    public static ClaimEvidenceLink Create(
        Guid claimId,
        Guid evidenceId,
        DateTimeOffset linkedAt)
    {
        if (claimId == Guid.Empty)
        {
            throw new ArgumentException("Claim id is required.", nameof(claimId));
        }

        if (evidenceId == Guid.Empty)
        {
            throw new ArgumentException("Evidence id is required.", nameof(evidenceId));
        }

        return new ClaimEvidenceLink(claimId, evidenceId, linkedAt);
    }
}
