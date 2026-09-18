namespace ContentOS.Application.Knowledge.Ports;

public sealed record SourceCaptureOutcome(
    bool IsSuccess,
    string? ErrorCode,
    Stream? Content,
    string? MediaType)
{
    public static SourceCaptureOutcome Ok(Stream content, string mediaType) =>
        new(true, null, content, mediaType);

    public static SourceCaptureOutcome Fail(string errorCode) =>
        new(false, errorCode, null, null);
}
