namespace ContentOS.Domain.Knowledge;

public readonly record struct EvidenceLocator
{
    private const int MaxLength = 2048;

    private EvidenceLocator(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static EvidenceLocator Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
        {
            throw new ArgumentException(
                $"Evidence locator must be at most {MaxLength} characters.",
                nameof(value));
        }

        return new EvidenceLocator(normalized);
    }

    public override string ToString() => Value;
}
