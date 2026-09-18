namespace ContentOS.Application.Content.Topics;

public sealed record StartTopicDiscoveryResult
{
    private StartTopicDiscoveryResult(
        bool isSuccess,
        bool isEmpty,
        bool isDuplicate,
        Guid? discoveryId,
        Guid? operationId,
        string? errorCode)
    {
        IsSuccess = isSuccess;
        IsEmpty = isEmpty;
        IsDuplicate = isDuplicate;
        DiscoveryId = discoveryId;
        OperationId = operationId;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }

    public bool IsEmpty { get; }

    public bool IsDuplicate { get; }

    public Guid? DiscoveryId { get; }

    public Guid? OperationId { get; }

    public string? ErrorCode { get; }

    public static StartTopicDiscoveryResult Started(Guid discoveryId, Guid? operationId, bool isEmpty) =>
        new(true, isEmpty, false, discoveryId, operationId, null);

    public static StartTopicDiscoveryResult Duplicate(Guid discoveryId) =>
        new(true, false, true, discoveryId, null, null);

    public static StartTopicDiscoveryResult Invalid(string errorCode) =>
        new(false, false, false, null, null, errorCode);
}
