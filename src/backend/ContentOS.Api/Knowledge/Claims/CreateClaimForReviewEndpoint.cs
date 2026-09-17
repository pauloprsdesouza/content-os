using ContentOS.Api.Http;
using ContentOS.Application.Knowledge.Claims.CreateForReview;
using ContentOS.Contracts.Knowledge;
using ContentOS.SharedKernel;

namespace ContentOS.Api.Knowledge.Claims;

public static class CreateClaimForReviewEndpoint
{
    public static RouteGroupBuilder MapCreateClaimForReview(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/",
                async (
                    CreateClaimForReviewRequest request,
                    CreateClaimForReviewHandler handler,
                    HttpContext httpContext,
                    CancellationToken cancellationToken) =>
                {
                    var idempotency = httpContext.Request.Headers.TryGetValue(
                        "Idempotency-Key",
                        out var key) && !string.IsNullOrWhiteSpace(key)
                        ? IdempotencyKey.Create(key.ToString())
                        : IdempotencyKey.Create(Guid.NewGuid().ToString("N"));

                    var result = await handler.HandleAsync(
                        new CreateClaimForReviewCommand(
                            request.Statement,
                            request.Confidence,
                            request.SnapshotId,
                            request.EvidenceLocator,
                            request.ExtractionMethod,
                            idempotency),
                        cancellationToken);

                    if (result.IsSuccess)
                    {
                        return Results.Created(
                            $"/api/v1/claims/{result.ClaimId}",
                            new CreateClaimForReviewResponse(
                                result.ClaimId!.Value,
                                result.EvidenceId!.Value));
                    }

                    return result.ErrorCode switch
                    {
                        "SNAPSHOT_NOT_FOUND" => Results.Problem(
                            statusCode: StatusCodes.Status404NotFound,
                            title: "Snapshot not found",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            }),
                        _ => Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Invalid claim",
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
