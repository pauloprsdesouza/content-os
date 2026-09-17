namespace ContentOS.Application.Knowledge.Claims.CreateForReview;

public sealed record CreateClaimForReviewResult
{
    private CreateClaimForReviewResult(
        bool isSuccess,
        Guid? claimId,
        Guid? evidenceId,
        string? errorCode)
    {
        IsSuccess = isSuccess;
        ClaimId = claimId;
        EvidenceId = evidenceId;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }

    public Guid? ClaimId { get; }

    public Guid? EvidenceId { get; }

    public string? ErrorCode { get; }

    public static CreateClaimForReviewResult Created(Guid claimId, Guid evidenceId) =>
        new(true, claimId, evidenceId, null);

    public static CreateClaimForReviewResult SnapshotNotFound() =>
        new(false, null, null, "SNAPSHOT_NOT_FOUND");

    public static CreateClaimForReviewResult InvalidInput(string errorCode) =>
        new(false, null, null, errorCode);
}
