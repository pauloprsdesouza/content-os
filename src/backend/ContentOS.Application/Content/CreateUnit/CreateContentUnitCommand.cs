namespace ContentOS.Application.Content.CreateUnit;

public sealed record CreateContentUnitCommand(
    string Title,
    string? Brief,
    bool QueueGeneration,
    Guid CreatedByUserId);
