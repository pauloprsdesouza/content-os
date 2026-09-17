using ContentOS.Domain.Content;

namespace ContentOS.Application.Content.GetVersion;

public sealed record GetContentVersionResult
{
    private GetContentVersionResult(bool isSuccess, ContentVersion? version, string? errorCode)
    {
        IsSuccess = isSuccess;
        Version = version;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }

    public ContentVersion? Version { get; }

    public string? ErrorCode { get; }

    public static GetContentVersionResult Found(ContentVersion version) =>
        new(true, version, null);

    public static GetContentVersionResult NotFound() =>
        new(false, null, "CONTENT_VERSION_NOT_FOUND");
}
