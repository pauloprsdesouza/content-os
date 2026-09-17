namespace ContentOS.Application.Publication.Ports;

public sealed record ApprovedContentVersionInfo(
    Guid Id,
    string? BodyMarkdown);
