using ContentOS.Application.Commerce.Ports;
using ContentOS.Application.Commerce.ReceiveKiwifyWebhook;
using ContentOS.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ContentOS.Infrastructure.Commerce;

/// <summary>
/// Development reconciler: treats the stored webhook payload as authoritative (ADR-0013 still requires this step).
/// </summary>
public sealed class StubCommerceOrderReconciler(IOptions<CommerceOptions> options) : ICommerceOrderReconciler
{
    public Task<CommerceOrderSnapshot> GetAuthoritativeOrderAsync(
        string provider,
        string externalId,
        string? webhookPayloadJson,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = provider;

        var payload = webhookPayloadJson ?? "{}";
        var status = KiwifyWebhookPayloadReader.TryReadString(payload, "order_status")
            ?? KiwifyWebhookPayloadReader.TryReadString(payload, "Status")
            ?? KiwifyWebhookPayloadReader.TryReadString(payload, "status")
            ?? "paid";

        var decision = MapDecision(status);
        var email = KiwifyWebhookPayloadReader.TryReadString(payload, "Customer", "email")
            ?? KiwifyWebhookPayloadReader.TryReadString(payload, "customer", "email")
            ?? KiwifyWebhookPayloadReader.TryReadString(payload, "email")
            ?? $"buyer+{externalId}@example.local";
        var name = KiwifyWebhookPayloadReader.TryReadString(payload, "Customer", "full_name")
            ?? KiwifyWebhookPayloadReader.TryReadString(payload, "customer", "full_name")
            ?? KiwifyWebhookPayloadReader.TryReadString(payload, "Customer", "name");

        Guid? productId = TryReadGuid(payload, "Product", "product_id")
            ?? TryReadGuid(payload, "product_id")
            ?? options.Value.DefaultProductId;
        Guid? editionId = TryReadGuid(payload, "edition_id")
            ?? options.Value.DefaultEditionId;

        return Task.FromResult(
            new CommerceOrderSnapshot(
                externalId,
                decision,
                email,
                name,
                productId,
                editionId));
    }

    private static CommerceReconcileDecision MapDecision(string status) =>
        status.Trim().ToLowerInvariant() switch
        {
            "paid" or "approved" or "complete" or "completed" => CommerceReconcileDecision.Confirm,
            "refunded" or "refund" => CommerceReconcileDecision.Refund,
            "chargeback" => CommerceReconcileDecision.Chargeback,
            _ => CommerceReconcileDecision.Ignore
        };

    private static Guid? TryReadGuid(string payload, params string[] path)
    {
        var raw = KiwifyWebhookPayloadReader.TryReadString(payload, path);
        return Guid.TryParse(raw, out var id) ? id : null;
    }
}
