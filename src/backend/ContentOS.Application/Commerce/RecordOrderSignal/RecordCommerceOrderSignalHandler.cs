using System.Text.RegularExpressions;
using ContentOS.Application.Commerce.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Commerce;
using ContentOS.SharedKernel;

namespace ContentOS.Application.Commerce.RecordOrderSignal;

public sealed partial class RecordCommerceOrderSignalHandler(
    IWebhookInboxRepository webhookInbox,
    IPurchaseRepository purchases,
    IIdGenerator ids,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<RecordCommerceOrderSignalResult> HandleAsync(
        RecordCommerceOrderSignalCommand command,
        CancellationToken cancellationToken = default)
    {
        var externalId = command.ExternalId.Trim();
        if (!OrderIdPattern().IsMatch(externalId))
        {
            return RecordCommerceOrderSignalResult.Invalid("COMMERCE_ORDER_ID_REQUIRED");
        }

        var existing = await purchases.GetByProviderExternalIdAsync(
            CommerceProviders.Kiwify,
            externalId,
            cancellationToken);
        if (existing is not null)
        {
            return RecordCommerceOrderSignalResult.Accepted(existing.Id, alreadyExisted: true);
        }

        var now = clock.GetUtcNow();
        var inboxId = ids.NewId();
        var purchaseId = ids.NewId();
        var inbox = WebhookInboxEntry.Record(
            inboxId,
            CommerceProviders.Kiwify,
            externalId,
            $"{{\"order_id\":\"{externalId}\",\"origin\":\"operator\"}}",
            signatureVerified: false,
            now);
        var purchase = Purchase.CreateFromSignal(
            purchaseId,
            CommerceProviders.Kiwify,
            externalId,
            inboxId,
            buyerEmail: null,
            productId: null,
            editionId: null,
            now);

        webhookInbox.Add(inbox);
        purchases.Add(purchase);
        await changes.CommitAsync(cancellationToken);
        return RecordCommerceOrderSignalResult.Accepted(purchaseId, alreadyExisted: false);
    }

    [GeneratedRegex("^[A-Za-z0-9_-]{1,80}$")]
    private static partial Regex OrderIdPattern();
}
