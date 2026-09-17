using ContentOS.Api.Http;
using ContentOS.Application.Knowledge.Snapshots.Create;
using ContentOS.Contracts.Knowledge;
using ContentOS.SharedKernel;

namespace ContentOS.Api.Knowledge.Snapshots;

public static class CreateSnapshotEndpoint
{
    public static RouteGroupBuilder MapCreateSnapshot(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/{sourceId:guid}/snapshots",
                async (
                    Guid sourceId,
                    CreateSnapshotRequest request,
                    CreateSnapshotHandler handler,
                    HttpContext httpContext,
                    CancellationToken cancellationToken) =>
                {
                    byte[] bytes;
                    try
                    {
                        bytes = Convert.FromBase64String(request.ContentBase64);
                    }
                    catch (FormatException)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Invalid snapshot payload",
                            detail: "contentBase64 must be valid Base64.",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = "SNAPSHOT_INVALID_CONTENT"
                            });
                    }

                    await using var stream = new MemoryStream(bytes, writable: false);
                    var idempotency = httpContext.Request.Headers.TryGetValue(
                        "Idempotency-Key",
                        out var key) && !string.IsNullOrWhiteSpace(key)
                        ? IdempotencyKey.Create(key.ToString())
                        : IdempotencyKey.Create(Guid.NewGuid().ToString("N"));

                    var result = await handler.HandleAsync(
                        new CreateSnapshotCommand(
                            sourceId,
                            stream,
                            request.MediaType,
                            idempotency),
                        cancellationToken);

                    if (result.IsSuccess)
                    {
                        return Results.Created(
                            $"/api/v1/sources/{sourceId}/snapshots",
                            new CreateSnapshotResponse(
                                result.SnapshotId!.Value,
                                result.ContentHash!));
                    }

                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Source not found",
                        extensions: new Dictionary<string, object?>
                        {
                            ["code"] = result.ErrorCode
                        });
                })
            .RequireAuthorization()
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }
}
