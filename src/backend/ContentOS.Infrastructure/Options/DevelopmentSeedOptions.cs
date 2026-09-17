namespace ContentOS.Infrastructure.Options;

public sealed class DevelopmentSeedOptions
{
    public const string SectionName = "DevelopmentSeed";

    public bool Enabled { get; init; }

    public string AdminEmail { get; init; } = string.Empty;

    public string AdminPassword { get; init; } = string.Empty;

    public bool SeedDemoKnowledge { get; init; } = true;
}
