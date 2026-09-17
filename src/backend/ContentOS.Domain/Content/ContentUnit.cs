namespace ContentOS.Domain.Content;

public sealed class ContentUnit
{
    private ContentUnit(
        Guid id,
        string title,
        string? brief,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        Id = id;
        Title = title;
        Brief = brief;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private ContentUnit()
    {
        Title = null!;
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; }

    public string? Brief { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static ContentUnit Create(
        Guid id,
        string title,
        string? brief,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Content unit id is required.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        if (createdByUserId == Guid.Empty)
        {
            throw new ArgumentException("Creator id is required.", nameof(createdByUserId));
        }

        return new ContentUnit(
            id,
            title.Trim(),
            string.IsNullOrWhiteSpace(brief) ? null : brief.Trim(),
            createdByUserId,
            createdAt);
    }

    public void Rename(string title, string? brief, DateTimeOffset at)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        Title = title.Trim();
        Brief = string.IsNullOrWhiteSpace(brief) ? null : brief.Trim();
        UpdatedAt = at;
    }
}
