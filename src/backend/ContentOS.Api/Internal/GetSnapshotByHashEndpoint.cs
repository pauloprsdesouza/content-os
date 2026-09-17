using ContentOS.Application.Knowledge.Ports;
using ContentOS.Contracts.Internal;
using ContentOS.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ContentOS.Api.Internal;

public static class GetSnapshotByHashEndpoint
{
    public const string WorkerKeyHeaderName = "X-ContentOS-Worker-Key";

    public static RouteGroupBuilder MapGetSnapshotByHash(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/knowledge/snapshots/by-hash/{contentHash}",
            async (
                string contentHash,
                HttpContext httpContext,
                ISnapshotByHashQuery query,
                IOptions<AiWorkerOptions> aiWorkerOptions,
                CancellationToken cancellationToken) =>
            {
                var configuredKey = aiWorkerOptions.Value.InternalApiKey;
                if (string.IsNullOrWhiteSpace(configuredKey))
                {
                    return Results.NotFound();
                }

                if (!httpContext.Request.Headers.TryGetValue(WorkerKeyHeaderName, out var providedKey)
                    || !string.Equals(providedKey.ToString(), configuredKey, StringComparison.Ordinal))
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status401Unauthorized,
                        title: "Unauthorized",
                        extensions: new Dictionary<string, object?>
                        {
                            ["code"] = "WORKER_KEY_INVALID"
                        });
                }

                var snapshot = await query.GetByHashAsync(contentHash, cancellationToken);
                if (snapshot is null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Snapshot not found",
                        extensions: new Dictionary<string, object?>
                        {
                            ["code"] = "SNAPSHOT_NOT_FOUND"
                        });
                }

                return Results.Ok(
                    new SnapshotByHashResponse(
                        snapshot.SnapshotId,
                        snapshot.SourceId,
                        snapshot.ContentHash,
                        snapshot.MediaType,
                        snapshot.ByteLength,
                        snapshot.CapturedAt));
            })
            .AllowAnonymous();

        return group;
    }
}
