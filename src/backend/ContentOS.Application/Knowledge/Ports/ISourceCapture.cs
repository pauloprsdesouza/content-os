using ContentOS.Application.Knowledge.Capture;

namespace ContentOS.Application.Knowledge.Ports;

public interface ISourceCapture
{
    SourceCaptureMode Mode { get; }

    Task<SourceCaptureOutcome> CaptureAsync(
        SourceCaptureRequest request,
        CancellationToken cancellationToken = default);
}
