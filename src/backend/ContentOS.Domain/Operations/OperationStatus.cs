namespace ContentOS.Domain.Operations;

public enum OperationStatus
{
    Accepted = 0,
    Running = 1,
    Succeeded = 2,
    Failed = 3,
    Cancelled = 4
}
