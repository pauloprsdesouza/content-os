using ContentOS.Api.Http;
using ContentOS.Application.Catalog.DeleteProduct;

namespace ContentOS.Api.Catalog;

public static class DeleteProductEndpoint
{
    public static RouteGroupBuilder MapDeleteProduct(this RouteGroupBuilder group)
    {
        group.MapDelete(
                "/{productId:guid}",
                async (
                    Guid productId,
                    DeleteProductHandler handler,
                    CancellationToken cancellationToken) =>
                    DiscardHttp.ToResult(
                        await handler.HandleAsync(new DeleteProductCommand(productId), cancellationToken)))
            .RequireAuthorization()
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }
}
