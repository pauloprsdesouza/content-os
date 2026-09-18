using ContentOS.Application.Knowledge.Capture;
using ContentOS.Application.Knowledge.Ports;
using ContentOS.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ContentOS.Infrastructure.Knowledge;

public sealed class UploadedFileSourceCapture(IOptions<IngestOptions> options) : ISourceCapture
{
    private static readonly HashSet<string> AllowedMediaTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "text/plain",
        "text/markdown",
        "text/x-markdown",
        "application/pdf"
    };

    public SourceCaptureMode Mode => SourceCaptureMode.File;

    public async Task<SourceCaptureOutcome> CaptureAsync(
        SourceCaptureRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.FileContent is null)
        {
            return SourceCaptureOutcome.Fail("CAPTURE_EMPTY");
        }

        var mediaType = ResolveMediaType(request.FileMediaType, request.FileName);
        if (!AllowedMediaTypes.Contains(mediaType))
        {
            return SourceCaptureOutcome.Fail("CAPTURE_UNSUPPORTED_MEDIA");
        }

        var maxBytes = Math.Clamp(options.Value.MaxBytes, 1024, 5_000_000);
        var buffer = new byte[maxBytes + 1];
        var read = 0;
        while (read < buffer.Length)
        {
            var count = await request.FileContent.ReadAsync(buffer.AsMemory(read), cancellationToken);
            if (count == 0)
            {
                break;
            }

            read += count;
        }

        if (read == 0)
        {
            return SourceCaptureOutcome.Fail("CAPTURE_EMPTY");
        }

        if (read > maxBytes)
        {
            return SourceCaptureOutcome.Fail("CAPTURE_TOO_LARGE");
        }

        var storedType = mediaType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
            ? "application/pdf"
            : "text/plain";
        return SourceCaptureOutcome.Ok(new MemoryStream(buffer, 0, read), storedType);
    }

    private static string ResolveMediaType(string? mediaType, string? fileName)
    {
        var normalized = string.IsNullOrWhiteSpace(mediaType)
            ? string.Empty
            : mediaType.Split(';')[0].Trim();
        if (AllowedMediaTypes.Contains(normalized))
        {
            return normalized;
        }

        return Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant() switch
        {
            ".md" or ".markdown" => "text/markdown",
            ".txt" => "text/plain",
            ".pdf" => "application/pdf",
            _ => normalized
        };
    }
}
