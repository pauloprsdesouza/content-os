using ContentOS.Domain.Publication;

namespace ContentOS.Application.Publication.GetPackage;

public sealed record GetPublicationPackageResult
{
    private GetPublicationPackageResult(
        bool isSuccess,
        PublicationPackage? package,
        string? errorCode)
    {
        IsSuccess = isSuccess;
        Package = package;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }

    public PublicationPackage? Package { get; }

    public string? ErrorCode { get; }

    public static GetPublicationPackageResult Found(PublicationPackage package) =>
        new(true, package, null);

    public static GetPublicationPackageResult NotFound() =>
        new(false, null, "PUBLICATION_PACKAGE_NOT_FOUND");
}
