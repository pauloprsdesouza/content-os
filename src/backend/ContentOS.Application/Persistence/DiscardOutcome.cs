namespace ContentOS.Application.Persistence;

public sealed record DiscardOutcome(bool Deleted, string? ErrorCode)
{
    public static DiscardOutcome Ok() => new(true, null);

    public static DiscardOutcome Missing(string code) => new(false, code);

    public static DiscardOutcome Blocked(string code) => new(false, code);
}
