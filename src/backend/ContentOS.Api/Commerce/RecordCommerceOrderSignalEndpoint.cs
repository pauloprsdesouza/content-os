using ContentOS.Api.Http;
using ContentOS.Application.Commerce.RecordOrderSignal;
using ContentOS.Contracts.Commerce;

namespace ContentOS.Api.Commerce;

public static class RecordCommerceOrderSignalEndpoint
{
    public static RouteGroupBuilder MapRecordCommerceOrderSignal(this RouteGroupBuilder group)
    {
        group.MapPost(
                "/order-signals",
                async (
                    RecordCommerceOrderSignalRequest request,
                    RecordCommerceOrderSignalHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        new RecordCommerceOrderSignalCommand(request.ExternalId),
                        cancellationToken);
                    if (!result.IsSuccess)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status422UnprocessableEntity,
                            title: "Informe o ID do pedido da Kiwify",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            });
                    }

                    return Results.Accepted(
                        $"/api/v1/purchases/{result.PurchaseId}",
                        new { purchaseId = result.PurchaseId, alreadyExisted = result.AlreadyExisted });
                })
            .RequireAuthorization()
            .WithMetadata(RequireRequestAntiforgery.Instance);

        return group;
    }
}
