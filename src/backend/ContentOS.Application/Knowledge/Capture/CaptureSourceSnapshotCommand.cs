namespace ContentOS.Application.Knowledge.Capture;

public sealed record CaptureSourceSnapshotCommand(
    SourceCaptureMode Mode,
    Guid? SourceId,
    string? DisplayName,
    string? Location,
    string? PastedText,
    Stream? FileContent,
    string? FileMediaType,
    string? FileName);
