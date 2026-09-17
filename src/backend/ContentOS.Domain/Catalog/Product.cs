namespace ContentOS.Domain.Catalog;

public sealed class Product
{
    private Product(
        Guid id,
        string name,
        string? description,
        DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        Description = description;
        Version = 1;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private Product()
    {
        Name = null!;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public long Version { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Product Create(
        Guid id,
        string name,
        string? description,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Product id is required.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Product(
            id,
            name.Trim(),
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            createdAt);
    }
}
