using ContentOS.Domain.Publication;

namespace ContentOS.Application.Publication.ExportPackage;

public sealed record ExportPublicationPackageResult
{
    private ExportPublicationPackageResult(
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

    public static ExportPublicationPackageResult Exported(PublicationPackage package) =>
        new(true, package, null);

    public static ExportPublicationPackageResult NotFound() =>
        new(false, null, "PUBLICATION_PACKAGE_NOT_FOUND");

    public static ExportPublicationPackageResult InvalidState(string code) =>
        new(false, null, code);
}
