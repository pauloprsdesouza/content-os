using Microsoft.AspNetCore.Antiforgery;

namespace ContentOS.Api.Http;

public sealed class RequireRequestAntiforgery : IAntiforgeryMetadata
{
    public static RequireRequestAntiforgery Instance { get; } = new();

    public bool RequiresValidation => true;
}
