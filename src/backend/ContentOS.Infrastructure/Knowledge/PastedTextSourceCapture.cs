using System.Text;
using ContentOS.Application.Knowledge.Capture;
using ContentOS.Application.Knowledge.Ports;
using ContentOS.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ContentOS.Infrastructure.Knowledge;

public sealed class PastedTextSourceCapture(IOptions<IngestOptions> options) : ISourceCapture
{
    public SourceCaptureMode Mode => SourceCaptureMode.Text;

    public Task<SourceCaptureOutcome> CaptureAsync(
        SourceCaptureRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var text = request.PastedText?.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return Task.FromResult(SourceCaptureOutcome.Fail("CAPTURE_EMPTY"));
        }

        var bytes = Encoding.UTF8.GetBytes(text);
        if (bytes.Length > Math.Clamp(options.Value.MaxBytes, 1024, 5_000_000))
        {
            return Task.FromResult(SourceCaptureOutcome.Fail("CAPTURE_TOO_LARGE"));
        }

        return Task.FromResult(
            SourceCaptureOutcome.Ok(new MemoryStream(bytes), "text/plain"));
    }
}
