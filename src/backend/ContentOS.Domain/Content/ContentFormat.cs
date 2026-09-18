namespace ContentOS.Domain.Content;

public sealed class ContentFormat : IEquatable<ContentFormat>
{
    public static readonly ContentFormat Newsletter = new("newsletter");

    public static readonly ContentFormat Post = new("post");

    public static readonly ContentFormat Lesson = new("aula");

    public static readonly ContentFormat Ebook = new("ebook");

    public static readonly ContentFormat Article = new("artigo");

    private static readonly ContentFormat[] Known =
    [
        Newsletter,
        Post,
        Lesson,
        Ebook,
        Article
    ];

    private ContentFormat(string code) => Code = code;

    public string Code { get; }

    public static bool TryParse(string? value, out ContentFormat format)
    {
        var match = Known.FirstOrDefault(item =>
            string.Equals(item.Code, value?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (match is null)
        {
            format = null!;
            return false;
        }

        format = match;
        return true;
    }

    public static ContentFormat Parse(string value) =>
        TryParse(value, out var format)
            ? format
            : throw new ArgumentException($"Unknown content format '{value}'.", nameof(value));

    public bool Equals(ContentFormat? other) =>
        other is not null && string.Equals(Code, other.Code, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is ContentFormat other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Code);

    public override string ToString() => Code;
}
