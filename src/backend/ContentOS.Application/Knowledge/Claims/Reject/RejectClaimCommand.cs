namespace ContentOS.Application.Knowledge.Claims.Reject;

public sealed record RejectClaimCommand(
    Guid ClaimId,
    Guid ActorUserId,
    string Reason,
    long ExpectedVersion);
