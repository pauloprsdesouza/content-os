using System.Security.Claims;
using ContentOS.Api.Http;
using ContentOS.Application.Knowledge.Claims.Approve;
using ContentOS.Domain.Identity;

namespace ContentOS.Api.Knowledge.Claims;

public static class ApproveClaimEndpoint
{
    public static RouteGroupBuilder MapApproveClaim(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/{claimId:guid}/approve",
                async (
                    Guid claimId,
                    ApproveClaimHandler handler,
                    HttpContext httpContext,
                    CancellationToken cancellationToken) =>
                {
                    if (!ETagFormatter.TryParse(
                            httpContext.Request.Headers.IfMatch.ToString(),
                            out var expectedVersion))
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status428PreconditionRequired,
                            title: "If-Match required",
                            detail: "Send the claim ETag via If-Match.",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = "CLAIM_IF_MATCH_REQUIRED"
                            });
                    }

                    var userIdValue = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!Guid.TryParse(userIdValue, out var userId))
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status401Unauthorized,
                            title: "Unauthenticated");
                    }

                    var result = await handler.HandleAsync(
                        new ApproveClaimCommand(claimId, userId, expectedVersion),
                        cancellationToken);

                    if (result.IsSuccess)
                    {
                        return Results.NoContent()
                            .WithETag(ETagFormatter.Format(result.NewVersion!.Value));
                    }

                    return result.ErrorCode switch
                    {
                        "CLAIM_NOT_FOUND" => Results.Problem(
                            statusCode: StatusCodes.Status404NotFound,
                            title: "Claim not found",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            }),
                        "CLAIM_STALE" => Results.Problem(
                            statusCode: StatusCodes.Status412PreconditionFailed,
                            title: "The claim changed",
                            detail: "Reload and review the latest version.",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode,
                                ["currentVersion"] = result.CurrentVersion
                            }),
                        "CLAIM_MISSING_EVIDENCE" => Results.Problem(
                            statusCode: StatusCodes.Status409Conflict,
                            title: "Evidence required",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            }),
                        _ => Results.Problem(
                            statusCode: StatusCodes.Status409Conflict,
                            title: "Claim cannot be approved",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            })
                    };
                })
            .RequireAuthorization(CapabilityNames.KnowledgeApprove)
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }
}
