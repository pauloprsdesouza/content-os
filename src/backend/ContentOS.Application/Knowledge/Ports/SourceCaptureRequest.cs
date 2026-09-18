namespace ContentOS.Application.Knowledge.Ports;

public sealed record SourceCaptureRequest(
    Uri? Location,
    string? PastedText,
    Stream? FileContent,
    string? FileMediaType,
    string? FileName);
