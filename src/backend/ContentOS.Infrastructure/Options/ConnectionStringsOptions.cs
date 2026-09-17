namespace ContentOS.Infrastructure.Options;

public sealed class ConnectionStringsOptions
{
    public const string SectionName = "ConnectionStrings";

    public string Platform { get; init; } = string.Empty;
}
