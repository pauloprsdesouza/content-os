namespace ContentOS.Application.Catalog.CreateProduct;

public sealed record CreateProductResult(
    bool IsSuccess,
    string? ErrorCode,
    Guid? ProductId,
    Guid? EditionId)
{
    public static CreateProductResult Created(Guid productId, Guid editionId) =>
        new(true, null, productId, editionId);

    public static CreateProductResult Invalid(string code) => new(false, code, null, null);
}
