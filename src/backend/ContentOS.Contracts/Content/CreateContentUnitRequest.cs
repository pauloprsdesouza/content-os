namespace ContentOS.Contracts.Content;

public sealed record CreateContentUnitRequest(
    string Title,
    string? Brief,
    bool QueueGeneration = true);
