using System.Security.Claims;
using ContentOS.Api.Http;
using ContentOS.Application.Research.Create;
using ContentOS.Contracts.Common;
using ContentOS.Contracts.Research;

namespace ContentOS.Api.Research;

public static class CreateResearchJobEndpoint
{
    public static RouteGroupBuilder MapCreateResearchJob(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/",
                async (
                    CreateResearchJobRequest request,
                    CreateResearchJobHandler handler,
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
                        new CreateResearchJobCommand(
                            request.Topic,
                            request.ScopeNotes,
                            userId,
                            request.SourceSnapshotId),
                        cancellationToken);

                    if (result.IsSuccess)
                    {
                        var operationId = result.OperationId!.Value;
                        var statusUrl = $"/api/v1/operations/{operationId}";
                        return Results.Accepted(
                            statusUrl,
                            new OperationAcceptedResponse(
                                operationId,
                                statusUrl,
                                result.ResearchJobId));
                    }

                    return result.ErrorCode switch
                    {
                        "RESEARCH_TOPIC_REQUIRED" => Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Topic required",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            }),
                        "RESEARCH_SNAPSHOT_REQUIRED" => Results.Problem(
                            statusCode: StatusCodes.Status422UnprocessableEntity,
                            title: "A pesquisa precisa de um snapshot existente",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            }),
                        _ => Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Invalid research job",
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
