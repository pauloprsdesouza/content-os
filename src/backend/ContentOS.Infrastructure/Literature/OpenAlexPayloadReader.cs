using System.Text.Json;
using ContentOS.Application.Content.Literature;

namespace ContentOS.Infrastructure.Literature;

public static class OpenAlexPayloadReader
{
    public static IReadOnlyList<ScholarlyArea> ReadAreas(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("results", out var results)
            || results.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var areas = new List<ScholarlyArea>();
        foreach (var item in results.EnumerateArray())
        {
            var name = ReadString(item, "display_name");
            if (!OpenAlexIdentifiers.TryNormalizeArea(ReadString(item, "id"), out var id, out _)
                || string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            areas.Add(new ScholarlyArea(id, name));
        }

        return areas;
    }

    public static IReadOnlyList<ScholarlyWork> ReadWorks(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("results", out var results)
            || results.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var works = new List<ScholarlyWork>();
        foreach (var item in results.EnumerateArray())
        {
            var workId = OpenAlexIdentifiers.ShortWorkId(ReadString(item, "id"));
            var title = ReadString(item, "display_name");
            if (workId is null || string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            int? year = item.TryGetProperty("publication_year", out var yearNode) && yearNode.TryGetInt32(out var parsedYear)
                ? parsedYear
                : null;
            string? topicId = null;
            string? topicName = null;
            if (item.TryGetProperty("primary_topic", out var topic) && topic.ValueKind == JsonValueKind.Object)
            {
                topicId = OpenAlexIdentifiers.ShortTopicId(ReadString(topic, "id"));
                topicName = ReadString(topic, "display_name");
            }

            var abstractText = item.TryGetProperty("abstract_inverted_index", out var index)
                ? ReconstructAbstract(index)
                : string.Empty;
            works.Add(new ScholarlyWork(workId, title, year, topicId, topicName, abstractText));
        }

        return works;
    }

    public static string ReconstructAbstract(JsonElement index)
    {
        if (index.ValueKind != JsonValueKind.Object)
        {
            return string.Empty;
        }

        var positions = new SortedDictionary<int, string>();
        foreach (var word in index.EnumerateObject())
        {
            if (word.Value.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var position in word.Value.EnumerateArray())
            {
                if (position.TryGetInt32(out var indexPosition))
                {
                    positions[indexPosition] = word.Name;
                }
            }
        }

        return string.Join(' ', positions.Values);
    }

    private static string? ReadString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var node) && node.ValueKind == JsonValueKind.String
            ? node.GetString()
            : null;
}
