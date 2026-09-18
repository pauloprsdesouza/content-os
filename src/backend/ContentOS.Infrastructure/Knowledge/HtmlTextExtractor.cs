using System.Net;
using System.Text.RegularExpressions;

namespace ContentOS.Infrastructure.Knowledge;

public static partial class HtmlTextExtractor
{
    public static string Extract(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var withoutScripts = ScriptPattern().Replace(html, " ");
        var withoutStyles = StylePattern().Replace(withoutScripts, " ");
        var withoutTags = TagPattern().Replace(withoutStyles, " ");
        var decoded = WebUtility.HtmlDecode(withoutTags);
        return WhitespacePattern().Replace(decoded, " ").Trim();
    }

    [GeneratedRegex("<script\\b[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ScriptPattern();

    [GeneratedRegex("<style\\b[^>]*>.*?</style>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex StylePattern();

    [GeneratedRegex("<[^>]+>", RegexOptions.Singleline)]
    private static partial Regex TagPattern();

    [GeneratedRegex("\\s+")]
    private static partial Regex WhitespacePattern();
}
