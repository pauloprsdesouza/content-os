namespace ContentOS.Domain.Learning;

public sealed class Learner
{
    private Learner(
        Guid id,
        string email,
        string? displayName,
        DateTimeOffset createdAt)
    {
        Id = id;
        Email = email;
        DisplayName = displayName;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private Learner()
    {
        Email = null!;
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; }

    public string? DisplayName { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Learner Create(
        Guid id,
        string email,
        string? displayName,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Learner id is required.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return new Learner(
            id,
            email.Trim().ToLowerInvariant(),
            string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim(),
            createdAt);
    }
}
