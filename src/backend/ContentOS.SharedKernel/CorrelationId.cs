namespace ContentOS.SharedKernel;

public readonly record struct CorrelationId
{
    private CorrelationId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static CorrelationId Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new CorrelationId(value.Trim());
    }

    public override string ToString() => Value;
}
