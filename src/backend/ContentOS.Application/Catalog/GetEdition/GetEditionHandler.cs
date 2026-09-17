using ContentOS.Application.Catalog.Ports;

namespace ContentOS.Application.Catalog.GetEdition;

public sealed class GetEditionHandler(IEditionDetailQuery editions)
{
    public async Task<GetEditionResult> HandleAsync(
        GetEditionQuery query,
        CancellationToken cancellationToken = default)
    {
        var edition = await editions.GetAsync(
            query.ProductId,
            query.EditionId,
            cancellationToken);

        return edition is null
            ? GetEditionResult.NotFound()
            : GetEditionResult.Found(edition);
    }
}
