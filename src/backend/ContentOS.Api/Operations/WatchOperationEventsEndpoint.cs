using System.Text.Json;
using ContentOS.Application.Operations.Get;
using ContentOS.Application.Operations.Progress;
using ContentOS.Contracts.Common;
using ContentOS.Domain.Operations;
using Microsoft.AspNetCore.Http.Features;

namespace ContentOS.Api.Operations;

public static class WatchOperationEventsEndpoint
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly HashSet<string> TerminalStatuses = new(StringComparer.Ordinal)
    {
        nameof(OperationStatus.Succeeded),
        nameof(OperationStatus.Failed),
        nameof(OperationStatus.Cancelled)
    };

    public static RouteGroupBuilder MapWatchOperationEvents(this RouteGroupBuilder group)
    {
        group.MapGet(
                "/{id:guid}/events",
                async (
                    Guid id,
                    HttpContext httpContext,
                    IServiceScopeFactory scopes,
                    IOperationProgress progress,
                    CancellationToken cancellationToken) =>
                {
                    await using var subscription = progress.Subscribe(id);
                    OperationResponse? snapshot;
                    await using (var scope = scopes.CreateAsyncScope())
                    {
                        var handler = scope.ServiceProvider.GetRequiredService<GetOperationHandler>();
                        var result = await handler.HandleAsync(new GetOperationQuery(id), cancellationToken);
                        if (!result.IsSuccess || result.Operation is null)
                        {
                            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                            await httpContext.Response.WriteAsJsonAsync(
                                new
                                {
                                    title = "Operation not found",
                                    status = StatusCodes.Status404NotFound,
                                    code = "OPERATION_NOT_FOUND"
                                },
                                JsonOptions,
                                cancellationToken);
                            return;
                        }

                        var operation = result.Operation;
                        snapshot = new OperationResponse(
                            operation.Id,
                            operation.Kind,
                            operation.SubjectType,
                            operation.SubjectId,
                            operation.Status.ToString(),
                            operation.ErrorMessage,
                            operation.CreatedAt,
                            operation.UpdatedAt);
                    }

                    if (snapshot is null)
                    {
                        return;
                    }

                    var response = httpContext.Response;
                    response.StatusCode = StatusCodes.Status200OK;
                    response.ContentType = "text/event-stream";
                    response.Headers.CacheControl = "no-cache";
                    response.Headers.Append("X-Accel-Buffering", "no");
                    httpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

                    await WriteOperationAsync(response, snapshot, cancellationToken);
                    if (TerminalStatuses.Contains(snapshot.Status))
                    {
                        return;
                    }

                    while (subscription.TryRead(out var missed))
                    {
                        await WriteNoticeAsync(response, snapshot, missed, cancellationToken);
                        if (TerminalStatuses.Contains(missed.Status))
                        {
                            return;
                        }
                    }

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            using var heartbeat = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                            heartbeat.CancelAfter(TimeSpan.FromSeconds(15));
                            if (!await subscription.WaitToReadAsync(heartbeat.Token))
                            {
                                return;
                            }
                        }
                        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                        {
                            await response.WriteAsync(": ping\n\n", cancellationToken);
                            await response.Body.FlushAsync(cancellationToken);
                            continue;
                        }

                        while (subscription.TryRead(out var notice))
                        {
                            await WriteNoticeAsync(response, snapshot, notice, cancellationToken);
                            if (TerminalStatuses.Contains(notice.Status))
                            {
                                return;
                            }
                        }
                    }
                })
            .RequireAuthorization();

        return group;
    }

    private static Task WriteNoticeAsync(
        HttpResponse response,
        OperationResponse snapshot,
        OperationProgressNotice notice,
        CancellationToken cancellationToken) =>
        WriteOperationAsync(
            response,
            snapshot with
            {
                Status = notice.Status,
                ErrorMessage = notice.ErrorMessage,
                UpdatedAt = notice.UpdatedAt
            },
            cancellationToken);

    private static async Task WriteOperationAsync(
        HttpResponse response,
        OperationResponse operation,
        CancellationToken cancellationToken)
    {
        await response.WriteAsync("event: operation\n", cancellationToken);
        await response.WriteAsync("data: ", cancellationToken);
        await JsonSerializer.SerializeAsync(response.Body, operation, JsonOptions, cancellationToken);
        await response.WriteAsync("\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }
}
