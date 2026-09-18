using System.Security.Claims;
using ContentOS.Api.Http;
using ContentOS.Application.Content.CreateUnit;
using ContentOS.Contracts.Common;
using ContentOS.Contracts.Content;

namespace ContentOS.Api.Content;

public static class CreateContentUnitEndpoint
{
    public static RouteGroupBuilder MapCreateContentUnit(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/",
                async (
                    CreateContentUnitRequest request,
                    CreateContentUnitHandler handler,
                    HttpContext httpContext,
                    CancellationToken cancellationToken) =>
                {
                    var userIdValue = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!Guid.TryParse(userIdValue, out var userId))
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status401Unauthorized,
                            title: "Unauthenticated");
                    }

                    var result = await handler.HandleAsync(
                        new CreateContentUnitCommand(
                            request.Title,
                            request.Brief,
                            request.QueueGeneration,
                            userId,
                            request.Format,
                            request.CitationContentHashes,
                            ProductId: request.ProductId),
                        cancellationToken);

                    if (!result.IsSuccess)
                    {
                        return result.ErrorCode switch
                        {
                            "CONTENT_TITLE_REQUIRED" => Results.Problem(
                                statusCode: StatusCodes.Status400BadRequest,
                                title: "Title required",
                                extensions: new Dictionary<string, object?>
                                {
                                    ["code"] = result.ErrorCode
                                }),
                            "CONTENT_FORMAT_UNKNOWN" => Results.Problem(
                                statusCode: StatusCodes.Status400BadRequest,
                                title: "Format required",
                                extensions: new Dictionary<string, object?>
                                {
                                    ["code"] = result.ErrorCode
                                }),
                            _ => Results.Problem(
                                statusCode: StatusCodes.Status400BadRequest,
                                title: "Invalid content unit",
                                extensions: new Dictionary<string, object?>
                                {
                                    ["code"] = result.ErrorCode
                                })
                        };
                    }

                    var unitId = result.ContentUnitId!.Value;
                    var versionId = result.ContentVersionId!.Value;

                    if (result.OperationId is { } operationId)
                    {
                        var statusUrl = $"/api/v1/operations/{operationId}";
                        return Results.Accepted(
                            statusUrl,
                            new OperationAcceptedResponse(
                                operationId,
                                statusUrl,
                                unitId));
                    }

                    return Results.Created(
                        $"/api/v1/content-versions/{versionId}",
                        new CreateContentUnitResponse(unitId, versionId));
                })
            .RequireAuthorization()
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }
}
