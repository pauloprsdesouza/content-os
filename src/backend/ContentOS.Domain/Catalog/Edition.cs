namespace ContentOS.Domain.Catalog;

public sealed class Edition
{
    private readonly List<CurriculumItem> _curriculum = [];

    private Edition(
        Guid id,
        Guid productId,
        string name,
        DateTimeOffset createdAt)
    {
        Id = id;
        ProductId = productId;
        Name = name;
        Version = 1;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private Edition()
    {
        Name = null!;
    }

    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }

    public string Name { get; private set; }

    public long Version { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyList<CurriculumItem> Curriculum => _curriculum;

    public static Edition Create(
        Guid id,
        Guid productId,
        string name,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Edition id is required.", nameof(id));
        }

        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product id is required.", nameof(productId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Edition(id, productId, name.Trim(), createdAt);
    }

    public void ReplaceCurriculum(
        IReadOnlyList<Guid> contentVersionIds,
        long expectedVersion,
        DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(contentVersionIds);
        EnsureExpectedVersion(expectedVersion);

        if (contentVersionIds.Any(id => id == Guid.Empty))
        {
            throw new ArgumentException(
                "Curriculum cannot contain empty content version ids.",
                nameof(contentVersionIds));
        }

        if (contentVersionIds.Distinct().Count() != contentVersionIds.Count)
        {
            throw new InvalidOperationException(
                $"Edition {Id} curriculum cannot contain duplicate content version ids.");
        }

        _curriculum.Clear();
        for (var index = 0; index < contentVersionIds.Count; index++)
        {
            _curriculum.Add(CurriculumItem.Create(Id, index, contentVersionIds[index]));
        }

        Version++;
        Touch(at);
    }

    private void EnsureExpectedVersion(long expectedVersion)
    {
        if (expectedVersion != Version)
        {
            throw new InvalidOperationException(
                $"Edition {Id} version mismatch. Expected {expectedVersion}, actual {Version}.");
        }
    }

    private void Touch(DateTimeOffset at) => UpdatedAt = at;
}
