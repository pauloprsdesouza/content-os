using ContentOS.Application.Commerce.RunReconciliation;
using ContentOS.Contracts.Commerce;
using ContentOS.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ContentOS.Api.Commerce;

public static class RunReconciliationEndpoint
{
    public static RouteGroupBuilder MapRunReconciliation(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/",
                async (
                    RunReconciliationHandler handler,
                    IOptions<CommerceOptions> commerceOptions,
                    CancellationToken cancellationToken) =>
                {
                    var options = commerceOptions.Value;
                    var result = await handler.HandleAsync(
                        new RunReconciliationCommand(null),
                        options.DefaultProductId,
                        options.DefaultEditionId,
                        cancellationToken);

                    if (!result.IsSuccess)
                    {
                        var status = result.ErrorCode == "COMMERCE_PROVIDER_NOT_CONFIGURED"
                            ? StatusCodes.Status501NotImplemented
                            : StatusCodes.Status409Conflict;
                        return Results.Problem(
                            statusCode: status,
                            title: "Reconciliation failed",
                            detail: result.Detail,
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            });
                    }

                    return Results.Ok(
                        new ReconciliationRunResponse(
                            result.RunId!.Value,
                            result.ProcessedCount,
                            result.ConfirmedCount,
                            result.IgnoredCount));
                })
            .RequireAuthorization()
            .WithMetadata(Http.RequireRequestAntiforgery.Instance);

        return group;
    }
}
