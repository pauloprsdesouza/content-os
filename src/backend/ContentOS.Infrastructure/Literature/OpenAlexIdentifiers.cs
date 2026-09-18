using System.Text.RegularExpressions;

namespace ContentOS.Infrastructure.Literature;

public static partial class OpenAlexIdentifiers
{
    public static bool TryNormalizeArea(string? raw, out string shortId, out string kind)
    {
        shortId = string.Empty;
        kind = string.Empty;
        var match = AreaPattern().Match(raw?.Trim() ?? string.Empty);
        if (!match.Success)
        {
            return false;
        }

        kind = match.Groups["kind"].Value.ToLowerInvariant();
        shortId = $"{kind}/{match.Groups["id"].Value}";
        return true;
    }

    public static string? ShortWorkId(string? raw)
    {
        var match = WorkPattern().Match(raw?.Trim() ?? string.Empty);
        return match.Success ? match.Groups["id"].Value : null;
    }

    public static string? ShortTopicId(string? raw)
    {
        var match = TopicPattern().Match(raw?.Trim() ?? string.Empty);
        return match.Success ? match.Groups["id"].Value : null;
    }

    [GeneratedRegex(@"^(?:https://openalex\.org/)?(?<kind>fields|subfields)/(?<id>\d+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex AreaPattern();

    [GeneratedRegex(@"^(?:https://openalex\.org/)?(?<id>W\d+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex WorkPattern();

    [GeneratedRegex(@"^(?:https://openalex\.org/)?(?<id>T\d+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex TopicPattern();
}
