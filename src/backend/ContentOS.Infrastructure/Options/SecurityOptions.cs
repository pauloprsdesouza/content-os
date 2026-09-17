namespace ContentOS.Infrastructure.Options;

public sealed class SecurityOptions
{
    public const string SectionName = "Security";

    public string CookieName { get; init; } = "contentos.session";
    public TimeSpan SessionLifetime { get; init; } = TimeSpan.FromHours(8);
}
