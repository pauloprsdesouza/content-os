namespace ContentOS.Domain.Catalog;

public sealed class CurriculumItem
{
    private CurriculumItem(Guid editionId, int position, Guid contentVersionId)
    {
        EditionId = editionId;
        Position = position;
        ContentVersionId = contentVersionId;
    }

    private CurriculumItem()
    {
    }

    public Guid EditionId { get; private set; }

    public int Position { get; private set; }

    public Guid ContentVersionId { get; private set; }

    public static CurriculumItem Create(
        Guid editionId,
        int position,
        Guid contentVersionId)
    {
        if (editionId == Guid.Empty)
        {
            throw new ArgumentException("Edition id is required.", nameof(editionId));
        }

        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Position must be >= 0.");
        }

        if (contentVersionId == Guid.Empty)
        {
            throw new ArgumentException("Content version id is required.", nameof(contentVersionId));
        }

        return new CurriculumItem(editionId, position, contentVersionId);
    }
}
