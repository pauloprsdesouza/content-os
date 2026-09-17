namespace ContentOS.SharedKernel;

public readonly record struct IdempotencyKey
{
    private IdempotencyKey(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static IdempotencyKey Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new IdempotencyKey(value.Trim());
    }

    public override string ToString() => Value;
}
