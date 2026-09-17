using ContentOS.Application.Knowledge.Ports;

namespace ContentOS.Application.Knowledge.Sources.GetSource;

public sealed class GetSourceHandler(ISourceRepository sources)
{
    public async Task<GetSourceResult> HandleAsync(
        GetSourceQuery query,
        CancellationToken cancellationToken = default)
    {
        var source = await sources.GetByIdAsync(query.SourceId, cancellationToken);
        return source is null
            ? GetSourceResult.NotFound()
            : GetSourceResult.From(source);
    }
}
