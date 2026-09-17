namespace ContentOS.Contracts.Common;

public sealed record OperationAcceptedResponse(
    Guid OperationId,
    string StatusUrl,
    Guid? SubjectId = null);
