namespace ContentOS.Application.Blobs;

public interface IBlobStore
{
    Task<BlobWriteResult> PutAsync(
        Stream content,
        string mediaType,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(
        string sha256,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string sha256,
        CancellationToken cancellationToken = default);
}
