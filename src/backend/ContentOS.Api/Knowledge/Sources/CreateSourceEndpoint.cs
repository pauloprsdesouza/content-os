using ContentOS.Api.Http;
using ContentOS.Application.Knowledge.Sources.Create;
using ContentOS.Contracts.Knowledge;
using ContentOS.Domain.Knowledge;
using ContentOS.SharedKernel;

namespace ContentOS.Api.Knowledge.Sources;

public static class CreateSourceEndpoint
{
    public static RouteGroupBuilder MapCreateSource(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/",
                async (
                    CreateSourceRequest request,
                    CreateSourceHandler handler,
                    HttpContext httpContext,
                    CancellationToken cancellationToken) =>
                {
                    if (!Enum.TryParse<SourceKind>(request.Kind, ignoreCase: true, out var kind))
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Invalid source kind",
                            detail: "Kind must be Web, Upload, or Api.",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = "SOURCE_INVALID_KIND"
                            });
                    }

                    var idempotency = httpContext.Request.Headers.TryGetValue(
                        "Idempotency-Key",
                        out var key) && !string.IsNullOrWhiteSpace(key)
                        ? IdempotencyKey.Create(key.ToString())
                        : IdempotencyKey.Create(Guid.NewGuid().ToString("N"));

                    var result = await handler.HandleAsync(
                        new CreateSourceCommand(
                            request.Location,
                            kind,
                            request.DisplayName,
                            idempotency),
                        cancellationToken);

                    if (result.IsSuccess)
                    {
                        return Results.Created(
                            $"/api/v1/sources/{result.SourceId}",
                            new CreateSourceResponse(result.SourceId!.Value));
                    }

                    return result.ErrorCode switch
                    {
                        "SOURCE_DUPLICATE_URI" => Results.Problem(
                            statusCode: StatusCodes.Status409Conflict,
                            title: "Source already exists",
                            detail: "A source with this canonical URI already exists.",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            }),
                        _ => Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Invalid source",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            })
                    };
                })
            .RequireAuthorization()
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }
}
