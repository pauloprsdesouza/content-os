namespace ContentOS.Domain.Content;

public sealed class TopicDiscovery
{
    private TopicDiscovery(
        Guid id,
        ContentFormat format,
        string areaId,
        string areaName,
        bool areaIsSubfield,
        int windowDays,
        Guid ownerUserId,
        Guid? seriesId,
        DateTimeOffset? scheduledFor,
        DateTimeOffset createdAt)
    {
        Id = id;
        Format = format;
        AreaId = areaId;
        AreaName = areaName;
        AreaIsSubfield = areaIsSubfield;
        WindowDays = windowDays;
        OwnerUserId = ownerUserId;
        SeriesId = seriesId;
        ScheduledFor = scheduledFor;
        Status = TopicDiscoveryStatus.Collecting;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private TopicDiscovery()
    {
        Format = ContentFormat.Article;
        AreaId = null!;
        AreaName = null!;
    }

    public Guid Id { get; private set; }

    public ContentFormat Format { get; private set; }

    public string AreaId { get; private set; }

    public string AreaName { get; private set; }

    public bool AreaIsSubfield { get; private set; }

    public int WindowDays { get; private set; }

    public TopicDiscoveryStatus Status { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public Guid? SeriesId { get; private set; }

    public DateTimeOffset? ScheduledFor { get; private set; }

    public Guid? OperationId { get; private set; }

    public Guid? ContentUnitId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static TopicDiscovery Start(
        Guid id,
        ContentFormat format,
        string areaId,
        string areaName,
        bool areaIsSubfield,
        int windowDays,
        Guid ownerUserId,
        Guid? seriesId,
        DateTimeOffset? scheduledFor,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty || ownerUserId == Guid.Empty)
        {
            throw new ArgumentException("Discovery and owner ids are required.");
        }

        ArgumentNullException.ThrowIfNull(format);
        ArgumentException.ThrowIfNullOrWhiteSpace(areaId);
        ArgumentException.ThrowIfNullOrWhiteSpace(areaName);
        if (windowDays is not (7 or 30 or 90))
        {
            throw new ArgumentOutOfRangeException(nameof(windowDays));
        }

        return new TopicDiscovery(
            id,
            format,
            areaId.Trim(),
            areaName.Trim(),
            areaIsSubfield,
            windowDays,
            ownerUserId,
            seriesId,
            scheduledFor,
            createdAt);
    }

    public void BindOperation(Guid operationId, DateTimeOffset at)
    {
        if (operationId == Guid.Empty)
        {
            throw new ArgumentException("Operation id is required.", nameof(operationId));
        }

        OperationId = operationId;
        Status = TopicDiscoveryStatus.Collecting;
        UpdatedAt = at;
    }

    public void MarkAwaitingSelection(DateTimeOffset at)
    {
        Status = TopicDiscoveryStatus.AwaitingSelection;
        UpdatedAt = at;
    }

    public void MarkWriting(Guid contentUnitId, DateTimeOffset at)
    {
        if (contentUnitId == Guid.Empty)
        {
            throw new ArgumentException("Content unit id is required.", nameof(contentUnitId));
        }

        ContentUnitId = contentUnitId;
        Status = TopicDiscoveryStatus.Writing;
        UpdatedAt = at;
    }

    public void MarkReadyForReview(DateTimeOffset at)
    {
        if (Status != TopicDiscoveryStatus.Writing)
        {
            return;
        }

        Status = TopicDiscoveryStatus.ReadyForReview;
        UpdatedAt = at;
    }
}
