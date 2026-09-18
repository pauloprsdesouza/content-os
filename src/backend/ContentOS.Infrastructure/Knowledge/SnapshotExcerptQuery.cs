using System.Text;
using ContentOS.Application.Blobs;
using ContentOS.Application.Knowledge.Ports;
using ContentOS.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ContentOS.Infrastructure.Knowledge;

public sealed class SnapshotExcerptQuery(
    ISnapshotByHashQuery snapshots,
    IBlobStore blobs,
    IOptions<IngestOptions> options) : ISnapshotExcerptQuery
{
    public async Task<SnapshotExcerpt?> GetByHashAsync(
        string contentHash,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await snapshots.GetByHashAsync(contentHash, cancellationToken);
        if (snapshot is null || !IsText(snapshot.MediaType))
        {
            return null;
        }

        if (!await blobs.ExistsAsync(snapshot.ContentHash, cancellationToken))
        {
            return null;
        }

        await using var stream = await blobs.OpenReadAsync(snapshot.ContentHash, cancellationToken);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var text = await reader.ReadToEndAsync(cancellationToken);
        var maxChars = Math.Clamp(options.Value.ExcerptMaxChars, 500, 50_000);
        if (text.Length > maxChars)
        {
            text = text[..maxChars];
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return new SnapshotExcerpt(
            snapshot.SnapshotId,
            snapshot.ContentHash,
            snapshot.MediaType,
            text.Trim());
    }

    private static bool IsText(string mediaType) =>
        mediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase);
}
