using System.Text.Json;

namespace ContentOS.Application.Commerce.ReceiveKiwifyWebhook;

public static class KiwifyWebhookPayloadReader
{
    public static string? TryReadString(string rawBody, params string[] path)
    {
        try
        {
            using var document = JsonDocument.Parse(rawBody);
            if (!TryNavigate(document.RootElement, path, out var current))
            {
                return null;
            }

            return current.ValueKind == JsonValueKind.String
                ? current.GetString()
                : current.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False
                    ? current.ToString()
                    : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static bool TryNavigate(JsonElement root, string[] path, out JsonElement current)
    {
        current = root;
        foreach (var segment in path)
        {
            if (current.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (current.TryGetProperty(segment, out var next))
            {
                current = next;
                continue;
            }

            JsonElement? matched = null;
            foreach (var property in current.EnumerateObject())
            {
                if (string.Equals(property.Name, segment, StringComparison.OrdinalIgnoreCase))
                {
                    matched = property.Value;
                    break;
                }
            }

            if (matched is null)
            {
                return false;
            }

            current = matched.Value;
        }

        return true;
    }
}
