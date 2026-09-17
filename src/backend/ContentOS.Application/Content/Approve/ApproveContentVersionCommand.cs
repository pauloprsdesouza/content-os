namespace ContentOS.Application.Content.Approve;

public sealed record ApproveContentVersionCommand(
    Guid ContentVersionId,
    Guid ActorUserId,
    long ExpectedVersion);
