using System.Security.Claims;
using ContentOS.Api.Http;
using ContentOS.Application.Knowledge.Claims.Reject;
using ContentOS.Contracts.Knowledge;
using ContentOS.Domain.Identity;

namespace ContentOS.Api.Knowledge.Claims;

public static class RejectClaimEndpoint
{
    public static RouteGroupBuilder MapRejectClaim(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/{claimId:guid}/reject",
                async (
                    Guid claimId,
                    RejectClaimRequest request,
                    RejectClaimHandler handler,
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
                        new RejectClaimCommand(
                            claimId,
                            userId,
                            request.Reason,
                            expectedVersion),
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
                        "CLAIM_REJECTION_REASON_REQUIRED" => Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Rejection reason required",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            }),
                        _ => Results.Problem(
                            statusCode: StatusCodes.Status409Conflict,
                            title: "Claim cannot be rejected",
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
