using ContentOS.Application.Learning.SubmitCapstone;
using ContentOS.Contracts.Learning;
using ContentOS.Api.Http;

namespace ContentOS.Api.Learning;

public static class SubmitCapstoneEndpoint
{
    public static RouteGroupBuilder MapSubmitCapstone(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/{enrollmentId:guid}/capstone/submit",
                async (
                    Guid enrollmentId,
                    SubmitCapstoneHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        new SubmitCapstoneCommand(enrollmentId),
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
                                title: "Capstone cannot be submitted",
                                extensions: new Dictionary<string, object?>
                                {
                                    ["code"] = result.ErrorCode
                                })
                        };
                    }

                    return Results.Ok(new CapstoneActionResponse(result.CapstoneId!.Value, null));
                })
            .RequireAuthorization()
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }
}
