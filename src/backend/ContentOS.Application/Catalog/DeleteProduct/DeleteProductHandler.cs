using ContentOS.Application.Catalog.Ports;
using ContentOS.Application.Persistence;

namespace ContentOS.Application.Catalog.DeleteProduct;

public sealed class DeleteProductHandler(
    IProductRepository products,
    IEditionRepository editions,
    IProductUsageQuery usage,
    IChangeCommitter changes)
{
    public async Task<DiscardOutcome> HandleAsync(
        DeleteProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var product = await products.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return DiscardOutcome.Missing("PRODUCT_NOT_FOUND");
        }

        var productEditions = await editions.ListByProductAsync(command.ProductId, cancellationToken);
        var editionIds = productEditions.Select(edition => edition.Id).ToArray();
        if (await usage.IsReferencedAsync(command.ProductId, editionIds, cancellationToken))
        {
            return DiscardOutcome.Blocked("PRODUCT_IN_USE");
        }

        foreach (var edition in productEditions)
        {
            editions.Remove(edition);
        }

        products.Remove(product);
        await changes.CommitAsync(cancellationToken);
        return DiscardOutcome.Ok();
    }
}
