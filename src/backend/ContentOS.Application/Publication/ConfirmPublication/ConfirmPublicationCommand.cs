namespace ContentOS.Application.Publication.ConfirmPublication;

public sealed record ConfirmPublicationCommand(
    Guid PackageId,
    Guid ActorUserId,
    long ExpectedVersion);
