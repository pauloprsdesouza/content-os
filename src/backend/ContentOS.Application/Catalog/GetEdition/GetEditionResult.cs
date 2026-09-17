using ContentOS.Application.Catalog.Ports;

namespace ContentOS.Application.Catalog.GetEdition;

public sealed record GetEditionResult
{
    private GetEditionResult(bool isSuccess, EditionDetailReadModel? edition, string? errorCode)
    {
        IsSuccess = isSuccess;
        Edition = edition;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }

    public EditionDetailReadModel? Edition { get; }

    public string? ErrorCode { get; }

    public static GetEditionResult Found(EditionDetailReadModel edition) =>
        new(true, edition, null);

    public static GetEditionResult NotFound() =>
        new(false, null, "EDITION_NOT_FOUND");
}
