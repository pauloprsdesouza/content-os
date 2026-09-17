namespace ContentOS.Infrastructure.Options;

public sealed class BlobStoreOptions
{
    public const string SectionName = "BlobStore";

    public string RootPath { get; init; } = string.Empty;
}
