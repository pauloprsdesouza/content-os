using ContentOS.Api.Http;
using ContentOS.Application.Catalog.CreateProduct;
using ContentOS.Contracts.Catalog;

namespace ContentOS.Api.Catalog;

public static class CreateProductEndpoint
{
    public static RouteGroupBuilder MapCreateProduct(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/",
                async (
                    CreateProductRequest request,
                    CreateProductHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        new CreateProductCommand(request.Name, request.Description, request.EditionName),
                        cancellationToken);
                    if (!result.IsSuccess || result.ProductId is null || result.EditionId is null)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Informe o nome do produto.",
                            detail: "Informe o nome do produto.",
                            extensions: new Dictionary<string, object?> { ["code"] = result.ErrorCode });
                    }

                    return Results.Created(
                        $"/api/v1/products/{result.ProductId}",
                        new CreateProductResponse(result.ProductId.Value, result.EditionId.Value));
                })
            .RequireAuthorization()
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }
}
