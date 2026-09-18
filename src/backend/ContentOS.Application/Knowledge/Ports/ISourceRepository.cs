using ContentOS.Domain.Knowledge;

namespace ContentOS.Application.Knowledge.Ports;

public interface ISourceRepository
{
    Task<bool> CanonicalUriExistsAsync(
        SourceUri canonicalUri,
        CancellationToken cancellationToken = default);

    Task<Source?> GetByIdAsync(
        Guid sourceId,
        CancellationToken cancellationToken = default);

    Task<Source?> GetByCanonicalUriAsync(
        SourceUri canonicalUri,
        CancellationToken cancellationToken = default);

    void Add(Source source);
}
