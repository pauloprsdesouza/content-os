using ContentOS.Application.Knowledge.Ports;

namespace ContentOS.Application.Knowledge.Sources.GetSources;

public sealed class GetSourcesHandler(ISourcesQuery sourcesQuery)
{
    public async Task<GetSourcesResult> HandleAsync(
        GetSourcesQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var result = await sourcesQuery.GetPageAsync(page, pageSize, cancellationToken);
        return new GetSourcesResult(result);
    }
}
