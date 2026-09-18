namespace ContentOS.Infrastructure.Options;

public sealed class OpenAlexOptions
{
    public const string SectionName = "OpenAlex";

    public string BaseUrl { get; init; } = "https://api.openalex.org";

    public string? MailTo { get; init; }

    public string? ApiKey { get; init; }

    public int TimeoutSeconds { get; init; } = 20;

    public int MaxWorks { get; init; } = 25;
}
