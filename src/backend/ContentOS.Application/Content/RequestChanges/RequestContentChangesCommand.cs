namespace ContentOS.Application.Content.RequestChanges;

public sealed record RequestContentChangesCommand(
    Guid ContentVersionId,
    Guid ActorUserId,
    string Notes,
    long ExpectedVersion);
