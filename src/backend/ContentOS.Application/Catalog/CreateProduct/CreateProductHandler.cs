using ContentOS.Application.Catalog.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Catalog;
using ContentOS.SharedKernel;

namespace ContentOS.Application.Catalog.CreateProduct;

public sealed class CreateProductHandler(
    IProductRepository products,
    IEditionRepository editions,
    IIdGenerator ids,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<CreateProductResult> HandleAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name) || command.Name.Trim().Length > 200)
        {
            return CreateProductResult.Invalid("PRODUCT_NAME_REQUIRED");
        }

        var now = clock.GetUtcNow();
        var productId = ids.NewId();
        var editionId = ids.NewId();
        var editionName = string.IsNullOrWhiteSpace(command.EditionName)
            ? "Edição 1"
            : command.EditionName.Trim();
        products.Add(Product.Create(productId, command.Name, command.Description, now));
        editions.Add(Edition.Create(editionId, productId, editionName, now));
        await changes.CommitAsync(cancellationToken);
        return CreateProductResult.Created(productId, editionId);
    }
}
