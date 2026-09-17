namespace ContentOS.Api.Http;

public static class ETagFormatter
{
    public static string Format(long version) => $"\"{version}\"";

    public static bool TryParse(string? ifMatch, out long version)
    {
        version = 0;
        if (string.IsNullOrWhiteSpace(ifMatch))
        {
            return false;
        }

        var value = ifMatch.Trim();
        if (value.StartsWith('W') && value.Contains('/'))
        {
            return false;
        }

        if (value.StartsWith('"') && value.EndsWith('"') && value.Length >= 2)
        {
            value = value[1..^1];
        }

        return long.TryParse(value, out version);
    }
}
