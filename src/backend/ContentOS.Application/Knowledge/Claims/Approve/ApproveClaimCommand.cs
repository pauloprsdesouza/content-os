namespace ContentOS.Application.Knowledge.Claims.Approve;

public sealed record ApproveClaimCommand(
    Guid ClaimId,
    Guid ActorUserId,
    long ExpectedVersion);
