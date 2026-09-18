namespace ContentOS.Application.Operations.Progress;

public sealed record OperationProgressNotice(
    Guid OperationId,
    string Status,
    string? ErrorMessage,
    DateTimeOffset UpdatedAt);
