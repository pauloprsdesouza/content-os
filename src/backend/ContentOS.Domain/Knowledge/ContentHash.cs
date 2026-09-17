namespace ContentOS.Domain.Knowledge;

public readonly record struct ContentHash
{
    private ContentHash(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ContentHash Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length != 64 || !normalized.All(Uri.IsHexDigit))
        {
            throw new ArgumentException(
                "Content hash must be a 64-character SHA-256 hex string.",
                nameof(value));
        }

        return new ContentHash(normalized);
    }

    public override string ToString() => Value;
}
