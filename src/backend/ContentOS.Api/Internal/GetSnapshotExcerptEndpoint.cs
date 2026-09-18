using ContentOS.Application.Knowledge.Ports;
using ContentOS.Contracts.Internal;
using ContentOS.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ContentOS.Api.Internal;

public static class GetSnapshotExcerptEndpoint
{
    public static RouteGroupBuilder MapGetSnapshotExcerpt(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/knowledge/snapshots/by-hash/{contentHash}/excerpt",
            async (
                string contentHash,
                HttpContext httpContext,
                ISnapshotExcerptQuery query,
                IOptions<AiWorkerOptions> aiWorkerOptions,
                CancellationToken cancellationToken) =>
            {
                var configuredKey = aiWorkerOptions.Value.InternalApiKey;
                if (string.IsNullOrWhiteSpace(configuredKey))
                {
                    return Results.NotFound();
                }

                if (!httpContext.Request.Headers.TryGetValue(GetSnapshotByHashEndpoint.WorkerKeyHeaderName, out var providedKey)
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

                var excerpt = await query.GetByHashAsync(contentHash, cancellationToken);
                if (excerpt is null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Snapshot excerpt not found",
                        extensions: new Dictionary<string, object?>
                        {
                            ["code"] = "SNAPSHOT_EXCERPT_UNAVAILABLE"
                        });
                }

                return Results.Ok(
                    new SnapshotExcerptResponse(
                        excerpt.SnapshotId,
                        excerpt.ContentHash,
                        excerpt.MediaType,
                        excerpt.Text));
            })
            .AllowAnonymous();

        return group;
    }
}
