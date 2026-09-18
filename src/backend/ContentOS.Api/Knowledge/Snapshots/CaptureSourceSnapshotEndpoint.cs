using ContentOS.Api.Http;
using ContentOS.Application.Knowledge.Capture;
using ContentOS.Contracts.Knowledge;
using Microsoft.AspNetCore.Antiforgery;

namespace ContentOS.Api.Knowledge.Snapshots;

public static class CaptureSourceSnapshotEndpoint
{
    public static RouteGroupBuilder MapCaptureExistingSource(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/{sourceId:guid}/captures",
                async (
                    Guid sourceId,
                    CaptureSourceSnapshotRequest request,
                    CaptureSourceSnapshotHandler handler,
                    CancellationToken cancellationToken) =>
                    await ExecuteAsync(
                        handler,
                        request,
                        sourceId,
                        file: null,
                        cancellationToken))
            .RequireAuthorization()
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }

    public static RouteGroupBuilder MapCreateSourceCapture(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/source-captures",
                async (
                    CaptureSourceSnapshotRequest request,
                    CaptureSourceSnapshotHandler handler,
                    CancellationToken cancellationToken) =>
                    await ExecuteAsync(
                        handler,
                        request,
                        sourceId: null,
                        file: null,
                        cancellationToken))
            .RequireAuthorization()
            .WithMetadata(RequireRequestAntiforgery.Instance);

        group.MapPost(
                "/source-captures/files",
                async (
                    HttpContext httpContext,
                    CaptureSourceSnapshotHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    if (httpContext.Features.Get<IAntiforgeryValidationFeature>() is { IsValid: false })
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Token antiforgery inválido",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = "ANTIFORGERY_INVALID"
                            });
                    }

                    var form = await httpContext.Request.ReadFormAsync(cancellationToken);
                    var file = form.Files.GetFile("file");
                    if (file is null)
                    {
                        return Problem("CAPTURE_EMPTY", StatusCodes.Status422UnprocessableEntity);
                    }

                    var displayName = form["displayName"].ToString();
                    await using var stream = file.OpenReadStream();
                    return await ExecuteAsync(
                        handler,
                        new CaptureSourceSnapshotRequest(
                            "file",
                            string.IsNullOrWhiteSpace(displayName) ? null : displayName,
                            null,
                            null),
                        sourceId: null,
                        file,
                        cancellationToken,
                        stream);
                })
            .RequireAuthorization()
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }

    private static async Task<IResult> ExecuteAsync(
        CaptureSourceSnapshotHandler handler,
        CaptureSourceSnapshotRequest request,
        Guid? sourceId,
        IFormFile? file,
        CancellationToken cancellationToken,
        Stream? fileStream = null)
    {
        if (!TryParseMode(request.Mode, out var mode))
        {
            return Problem("CAPTURE_MODE_UNSUPPORTED", StatusCodes.Status400BadRequest);
        }

        var result = await handler.HandleAsync(
            new CaptureSourceSnapshotCommand(
                mode,
                sourceId,
                request.DisplayName ?? file?.FileName,
                request.Location,
                request.Text,
                fileStream,
                file?.ContentType,
                file?.FileName),
            cancellationToken);

        if (!result.IsSuccess)
        {
            var status = result.ErrorCode == "SOURCE_NOT_FOUND"
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status422UnprocessableEntity;
            return Problem(result.ErrorCode ?? "CAPTURE_EMPTY", status);
        }

        return Results.Created(
            $"/api/v1/sources/{result.SourceId}/snapshots/{result.SnapshotId}",
            new CaptureSourceSnapshotResponse(
                result.SourceId!.Value,
                result.SnapshotId!.Value,
                result.ContentHash!,
                result.SourceCreated));
    }

    private static bool TryParseMode(string? mode, out SourceCaptureMode parsed)
    {
        parsed = mode?.Trim().ToLowerInvariant() switch
        {
            "web" => SourceCaptureMode.Web,
            "file" => SourceCaptureMode.File,
            "text" => SourceCaptureMode.Text,
            _ => SourceCaptureMode.Web
        };
        return mode?.Trim().ToLowerInvariant() is "web" or "file" or "text";
    }

    private static IResult Problem(string code, int status) =>
        Results.Problem(
            statusCode: status,
            title: "Não foi possível capturar a fonte",
            extensions: new Dictionary<string, object?>
            {
                ["code"] = code
            });
}
