namespace ContentOS.Domain.Content;

public sealed class EditorialSeries
{
    private EditorialSeries(
        Guid id,
        ContentFormat format,
        string areaId,
        string areaName,
        bool areaIsSubfield,
        int windowDays,
        SeriesCadence cadence,
        Guid ownerUserId,
        DateTimeOffset createdAt)
    {
        Id = id;
        Format = format;
        AreaId = areaId;
        AreaName = areaName;
        AreaIsSubfield = areaIsSubfield;
        WindowDays = windowDays;
        Cadence = cadence;
        OwnerUserId = ownerUserId;
        CreatedAt = createdAt;
    }

    private EditorialSeries()
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

    public SeriesCadence Cadence { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public DateTimeOffset? NextCollectionAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static EditorialSeries Create(
        Guid id,
        ContentFormat format,
        string areaId,
        string areaName,
        bool areaIsSubfield,
        int windowDays,
        SeriesCadence cadence,
        Guid ownerUserId,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty || ownerUserId == Guid.Empty)
        {
            throw new ArgumentException("Series and owner ids are required.");
        }

        ArgumentNullException.ThrowIfNull(format);
        ArgumentException.ThrowIfNullOrWhiteSpace(areaId);
        ArgumentException.ThrowIfNullOrWhiteSpace(areaName);
        if (windowDays is not (7 or 30 or 90))
        {
            throw new ArgumentOutOfRangeException(nameof(windowDays));
        }

        return new EditorialSeries(
            id,
            format,
            areaId.Trim(),
            areaName.Trim(),
            areaIsSubfield,
            windowDays,
            cadence,
            ownerUserId,
            createdAt);
    }

    public DateTimeOffset? NextAfter(DateTimeOffset from) => Cadence switch
    {
        SeriesCadence.Weekly => from.AddDays(7),
        SeriesCadence.Monthly => from.AddMonths(1),
        _ => null
    };

    public void RememberNextCollection(DateTimeOffset at) => NextCollectionAt = at;
}
