namespace ContentOS.Application.Blobs;

public sealed record BlobWriteResult(string Sha256, long Length, string MediaType);
