using ContentOS.Api.Http;
using ContentOS.Application.Knowledge.Claims.GetClaim;
using ContentOS.Api.Knowledge;

namespace ContentOS.Api.Knowledge.Claims;

public static class GetClaimEndpoint
{
    public static RouteGroupBuilder MapGetClaim(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/{claimId:guid}",
            async (
                Guid claimId,
                GetClaimHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new GetClaimQuery(claimId),
                    cancellationToken);

                if (!result.Found || result.Detail is null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Claim not found",
                        extensions: new Dictionary<string, object?>
                        {
                            ["code"] = "CLAIM_NOT_FOUND"
                        });
                }

                var response = ClaimResponseMapper.ToResponse(result.Detail);
                return Results.Ok(response)
                    .WithETag(ETagFormatter.Format(result.Detail.Version));
            })
            .RequireAuthorization();

        return group;
    }
}
