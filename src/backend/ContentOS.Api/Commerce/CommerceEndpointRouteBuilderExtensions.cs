namespace ContentOS.Api.Commerce;

public static class CommerceEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapCommerceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var webhooks = endpoints.MapGroup("/api/v1/commerce/webhooks");
        webhooks.MapReceiveKiwifyWebhook();

        var reconciliation = endpoints.MapGroup("/api/v1/commerce/reconciliation-runs");
        reconciliation.MapRunReconciliation();

        var signals = endpoints.MapGroup("/api/v1/commerce");
        signals.MapRecordCommerceOrderSignal();

        var purchases = endpoints.MapGroup("/api/v1/purchases");
        purchases.MapListPurchases();

        return endpoints;
    }
}
