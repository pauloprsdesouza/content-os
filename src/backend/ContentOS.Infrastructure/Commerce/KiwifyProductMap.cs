namespace ContentOS.Infrastructure.Commerce;

public sealed class KiwifyProductMap
{
    private readonly Dictionary<string, (Guid ProductId, Guid EditionId)> _entries;

    private KiwifyProductMap(Dictionary<string, (Guid ProductId, Guid EditionId)> entries)
    {
        _entries = entries;
    }

    public static KiwifyProductMap Parse(string? raw)
    {
        var entries = new Dictionary<string, (Guid, Guid)>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new KiwifyProductMap(entries);
        }

        foreach (var pair in raw.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var sides = pair.Split('=', 2, StringSplitOptions.TrimEntries);
            if (sides.Length != 2)
            {
                continue;
            }

            var ids = sides[1].Split('|', StringSplitOptions.TrimEntries);
            if (ids.Length != 2
                || !Guid.TryParse(ids[0], out var productId)
                || !Guid.TryParse(ids[1], out var editionId))
            {
                continue;
            }

            entries[sides[0]] = (productId, editionId);
        }

        return new KiwifyProductMap(entries);
    }

    public bool TryGet(string? kiwifyProductId, out Guid productId, out Guid editionId)
    {
        productId = Guid.Empty;
        editionId = Guid.Empty;
        if (string.IsNullOrWhiteSpace(kiwifyProductId)
            || !_entries.TryGetValue(kiwifyProductId, out var mapped))
        {
            return false;
        }

        productId = mapped.ProductId;
        editionId = mapped.EditionId;
        return true;
    }
}
