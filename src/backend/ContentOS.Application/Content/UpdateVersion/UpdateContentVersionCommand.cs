namespace ContentOS.Application.Content.UpdateVersion;

public sealed record UpdateContentVersionCommand(
    Guid ContentVersionId,
    string BodyMarkdown,
    long ExpectedVersion);
