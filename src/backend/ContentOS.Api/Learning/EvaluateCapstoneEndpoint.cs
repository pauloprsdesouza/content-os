using ContentOS.Api.Http;
using ContentOS.Application.Learning.EvaluateCapstone;
using ContentOS.Contracts.Learning;

namespace ContentOS.Api.Learning;

public static class EvaluateCapstoneEndpoint
{
    public static RouteGroupBuilder MapEvaluateCapstone(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/{capstoneId:guid}/evaluate",
                async (
                    Guid capstoneId,
                    EvaluateCapstoneRequest request,
                    EvaluateCapstoneHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        new EvaluateCapstoneCommand(capstoneId, request.Passed, request.Score),
                        cancellationToken);

                    if (!result.IsSuccess)
                    {
                        return result.ErrorCode switch
                        {
                            "CAPSTONE_NOT_FOUND" => Results.Problem(
                                statusCode: StatusCodes.Status404NotFound,
                                title: "Capstone not found",
                                extensions: new Dictionary<string, object?>
                                {
                                    ["code"] = result.ErrorCode
                                }),
                            _ => Results.Problem(
                                statusCode: StatusCodes.Status409Conflict,
                                title: "Capstone cannot be evaluated",
                                extensions: new Dictionary<string, object?>
                                {
                                    ["code"] = result.ErrorCode
                                })
                        };
                    }

                    return Results.Ok(
                        new CapstoneActionResponse(capstoneId, result.OutcomeId));
                })
            .RequireAuthorization()
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }
}
