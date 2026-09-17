namespace ContentOS.Domain.Operations;

/// <summary>
/// Minimal durable async work handle for Studio polling (202 OperationAccepted).
/// Assumption: stored in identity schema as platform support, not a product module.
/// </summary>
public sealed class Operation
{
    private Operation(
        Guid id,
        string kind,
        string subjectType,
        Guid subjectId,
        Guid requestedByUserId,
        DateTimeOffset createdAt)
    {
        Id = id;
        Kind = kind;
        SubjectType = subjectType;
        SubjectId = subjectId;
        RequestedByUserId = requestedByUserId;
        Status = OperationStatus.Accepted;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private Operation()
    {
        Kind = null!;
        SubjectType = null!;
    }

    public Guid Id { get; private set; }

    public string Kind { get; private set; }

    public string SubjectType { get; private set; }

    public Guid SubjectId { get; private set; }

    public Guid RequestedByUserId { get; private set; }

    public OperationStatus Status { get; private set; }

    public string? ErrorMessage { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Operation Create(
        Guid id,
        string kind,
        string subjectType,
        Guid subjectId,
        Guid requestedByUserId,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Operation id is required.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectType);

        if (subjectId == Guid.Empty)
        {
            throw new ArgumentException("Subject id is required.", nameof(subjectId));
        }

        if (requestedByUserId == Guid.Empty)
        {
            throw new ArgumentException("Requester id is required.", nameof(requestedByUserId));
        }

        return new Operation(
            id,
            kind.Trim(),
            subjectType.Trim(),
            subjectId,
            requestedByUserId,
            createdAt);
    }

    public void MarkRunning(DateTimeOffset at)
    {
        if (Status is not (OperationStatus.Accepted or OperationStatus.Running))
        {
            throw new InvalidOperationException(
                $"Operation {Id} cannot move to Running from {Status}.");
        }

        Status = OperationStatus.Running;
        ErrorMessage = null;
        UpdatedAt = at;
    }

    public void MarkSucceeded(DateTimeOffset at)
    {
        if (Status is OperationStatus.Succeeded or OperationStatus.Cancelled)
        {
            throw new InvalidOperationException(
                $"Operation {Id} cannot succeed from {Status}.");
        }

        Status = OperationStatus.Succeeded;
        ErrorMessage = null;
        UpdatedAt = at;
    }

    public void MarkFailed(string errorMessage, DateTimeOffset at)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        if (Status is OperationStatus.Succeeded or OperationStatus.Cancelled)
        {
            throw new InvalidOperationException(
                $"Operation {Id} cannot fail from {Status}.");
        }

        Status = OperationStatus.Failed;
        ErrorMessage = errorMessage.Trim();
        UpdatedAt = at;
    }

    public void Cancel(DateTimeOffset at)
    {
        if (Status is OperationStatus.Succeeded or OperationStatus.Cancelled)
        {
            throw new InvalidOperationException(
                $"Operation {Id} cannot be cancelled from {Status}.");
        }

        Status = OperationStatus.Cancelled;
        UpdatedAt = at;
    }
}
