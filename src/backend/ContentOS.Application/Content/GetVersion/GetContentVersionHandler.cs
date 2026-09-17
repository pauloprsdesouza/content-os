using ContentOS.Application.Content.Ports;

namespace ContentOS.Application.Content.GetVersion;

public sealed class GetContentVersionHandler(IContentVersionRepository versions)
{
    public async Task<GetContentVersionResult> HandleAsync(
        GetContentVersionQuery query,
        CancellationToken cancellationToken = default)
    {
        var version = await versions.GetByIdAsync(query.ContentVersionId, cancellationToken);
        return version is null
            ? GetContentVersionResult.NotFound()
            : GetContentVersionResult.Found(version);
    }
}
