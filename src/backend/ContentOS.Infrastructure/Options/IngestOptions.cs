namespace ContentOS.Infrastructure.Options;

public sealed class IngestOptions
{
    public const string SectionName = "Ingest";

    public int TimeoutSeconds { get; init; } = 15;

    public int MaxBytes { get; init; } = 1_048_576;

    public int MaxRedirects { get; init; } = 3;

    public int ExcerptMaxChars { get; init; } = 12_000;

    public int MinimumExtractedChars { get; init; } = 40;
}
