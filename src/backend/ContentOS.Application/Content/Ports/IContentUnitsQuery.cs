namespace ContentOS.Application.Content.Ports;

public interface IContentUnitsQuery
{
    Task<ContentUnitsPage> GetPageAsync(
        int page,
        int pageSize,
        Guid? productId,
        CancellationToken cancellationToken = default);
}
