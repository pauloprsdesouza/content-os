namespace ContentOS.Domain.Knowledge;

public readonly record struct ConfidenceScore
{
    private ConfidenceScore(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public static ConfidenceScore Create(decimal value)
    {
        if (value is < 0m or > 1m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "Confidence score must be between 0 and 1 inclusive.");
        }

        return new ConfidenceScore(decimal.Round(value, 4, MidpointRounding.AwayFromZero));
    }

    public override string ToString() => Value.ToString("0.####");
}
