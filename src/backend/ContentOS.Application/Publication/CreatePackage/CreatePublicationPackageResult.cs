using ContentOS.Domain.Publication;

namespace ContentOS.Application.Publication.CreatePackage;

public sealed record CreatePublicationPackageResult
{
    private CreatePublicationPackageResult(
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

    public static CreatePublicationPackageResult Created(PublicationPackage package) =>
        new(true, package, null);

    public static CreatePublicationPackageResult NotFound() =>
        new(false, null, "EDITION_NOT_FOUND");

    public static CreatePublicationPackageResult InvalidCurriculum(string code) =>
        new(false, null, code);
}
