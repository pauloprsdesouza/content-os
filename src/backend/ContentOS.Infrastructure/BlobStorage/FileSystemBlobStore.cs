using System.Buffers;
using System.Security.Cryptography;
using ContentOS.Application.Blobs;
using ContentOS.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ContentOS.Infrastructure.Blobs;

public sealed class FileSystemBlobStore : IBlobStore
{
    private const int BufferSize = 81920;
    private readonly string _rootPath;

    public FileSystemBlobStore(IOptions<BlobStoreOptions> options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Value.RootPath);
        _rootPath = Path.GetFullPath(options.Value.RootPath);
    }

    public async Task<BlobWriteResult> PutAsync(
        Stream content,
        string mediaType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(mediaType);

        var temporaryDirectory = Path.Combine(_rootPath, ".tmp");
        Directory.CreateDirectory(temporaryDirectory);
        var temporaryPath = Path.Combine(temporaryDirectory, Path.GetRandomFileName());

        try
        {
            var (hash, length) = await WriteAndHashAsync(
                content,
                temporaryPath,
                cancellationToken);
            var destinationPath = GetBlobPath(hash);

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

            if (!File.Exists(destinationPath))
            {
                try
                {
                    File.Move(temporaryPath, destinationPath);
                }
                catch (IOException) when (File.Exists(destinationPath))
                {
                }
            }

            return new BlobWriteResult(hash, length, mediaType);
        }
        finally
        {
            File.Delete(temporaryPath);
        }
    }

    public Task<Stream> OpenReadAsync(
        string sha256,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Stream stream = new FileStream(
            GetBlobPath(sha256),
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            BufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }

    public Task<bool> ExistsAsync(
        string sha256,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(File.Exists(GetBlobPath(sha256)));
    }

    private static async Task<(string Hash, long Length)> WriteAndHashAsync(
        Stream content,
        string temporaryPath,
        CancellationToken cancellationToken)
    {
        using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        await using var destination = new FileStream(
            temporaryPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            BufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        long length = 0;

        try
        {
            int bytesRead;
            while ((bytesRead = await content.ReadAsync(
                       buffer.AsMemory(0, BufferSize),
                       cancellationToken)) > 0)
            {
                await destination.WriteAsync(
                    buffer.AsMemory(0, bytesRead),
                    cancellationToken);
                hasher.AppendData(buffer, 0, bytesRead);
                length += bytesRead;
            }

            await destination.FlushAsync(cancellationToken);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        var hash = Convert.ToHexString(hasher.GetHashAndReset()).ToLowerInvariant();
        return (hash, length);
    }

    private string GetBlobPath(string sha256)
    {
        ValidateHash(sha256);
        return Path.Combine(_rootPath, sha256[..2], sha256);
    }

    private static void ValidateHash(string sha256)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sha256);

        if (sha256.Length != 64 || !sha256.All(Uri.IsHexDigit))
        {
            throw new ArgumentException("A lowercase or uppercase SHA-256 hex value is required.", nameof(sha256));
        }
    }
}
