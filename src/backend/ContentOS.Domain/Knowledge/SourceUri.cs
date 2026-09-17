namespace ContentOS.Domain.Knowledge;

public readonly record struct SourceUri
{
    private SourceUri(Uri value)
    {
        Value = value;
    }

    public Uri Value { get; }

    public static SourceUri Create(Uri location)
    {
        ArgumentNullException.ThrowIfNull(location);

        if (!location.IsAbsoluteUri)
        {
            throw new ArgumentException("Source URI must be absolute.", nameof(location));
        }

        if (location.Scheme is not ("http" or "https" or "file" or "contentos"))
        {
            throw new ArgumentException(
                "Source URI scheme must be http, https, file, or contentos.",
                nameof(location));
        }

        var builder = new UriBuilder(location)
        {
            Fragment = string.Empty
        };

        if (builder.Scheme is "http" or "https")
        {
            builder.Host = builder.Host.ToLowerInvariant();
        }

        return new SourceUri(builder.Uri);
    }

    public static SourceUri Create(string location)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(location);
        return Create(new Uri(location.Trim(), UriKind.Absolute));
    }

    public override string ToString() => Value.AbsoluteUri;
}
