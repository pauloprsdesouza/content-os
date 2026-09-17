namespace ContentOS.Infrastructure.Options;

public sealed class AiWorkerOptions
{
    public const string SectionName = "AiWorker";

    /// <summary>
    /// Shared secret for internal knowledge tool API. Empty disables the endpoint.
    /// </summary>
    public string InternalApiKey { get; init; } = string.Empty;
}
