namespace ContentOS.Application.Knowledge.Ports;

public interface ISourcesQuery
{
    Task<SourcesPage> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
