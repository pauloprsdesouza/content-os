using ContentOS.Application.Commerce.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Commerce;
using ContentOS.SharedKernel;

namespace ContentOS.Application.Commerce.ReceiveKiwifyWebhook;

public sealed class ReceiveKiwifyWebhookHandler(
    IWebhookInboxRepository webhookInbox,
    IPurchaseRepository purchases,
    IWebhookSignatureVerifier signatureVerifier,
    IIdGenerator ids,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<ReceiveKiwifyWebhookResult> HandleAsync(
        ReceiveKiwifyWebhookCommand command,
        string? webhookSecret,
        CancellationToken cancellationToken = default)
    {
        var signatureVerified = false;
        if (!string.IsNullOrWhiteSpace(webhookSecret))
        {
            if (!signatureVerifier.IsValid(command.RawBody, command.SignatureHeader, webhookSecret))
            {
                return ReceiveKiwifyWebhookResult.InvalidSignature();
            }

            signatureVerified = true;
        }
        else if (command.RequireSignature)
        {
            return ReceiveKiwifyWebhookResult.InvalidSignature();
        }

        if (!TryParseExternalId(command.RawBody, out var externalId))
        {
            return ReceiveKiwifyWebhookResult.InvalidPayload("COMMERCE_WEBHOOK_MISSING_EXTERNAL_ID");
        }

        var existing = await purchases.GetByProviderExternalIdAsync(
            CommerceProviders.Kiwify,
            externalId,
            cancellationToken);
        if (existing is not null)
        {
            return ReceiveKiwifyWebhookResult.Accepted(
                existing.Id,
                existing.WebhookInboxEntryId ?? Guid.Empty,
                alreadyExisted: true);
        }

        var existingInbox = await webhookInbox.GetByProviderExternalIdAsync(
            CommerceProviders.Kiwify,
            externalId,
            cancellationToken);
        if (existingInbox is not null)
        {
            var linked = await purchases.GetByProviderExternalIdAsync(
                CommerceProviders.Kiwify,
                externalId,
                cancellationToken);
            if (linked is not null)
            {
                return ReceiveKiwifyWebhookResult.Accepted(
                    linked.Id,
                    existingInbox.Id,
                    alreadyExisted: true);
            }
        }

        var now = clock.GetUtcNow();
        var inboxId = ids.NewId();
        var purchaseId = ids.NewId();
        var buyerEmail = KiwifyWebhookPayloadReader.TryReadString(command.RawBody, "Customer", "email")
            ?? KiwifyWebhookPayloadReader.TryReadString(command.RawBody, "customer", "email")
            ?? KiwifyWebhookPayloadReader.TryReadString(command.RawBody, "email");

        var inbox = WebhookInboxEntry.Record(
            inboxId,
            CommerceProviders.Kiwify,
            externalId,
            command.RawBody,
            signatureVerified,
            now);
        var purchase = Purchase.CreateFromSignal(
            purchaseId,
            CommerceProviders.Kiwify,
            externalId,
            inboxId,
            buyerEmail,
            productId: null,
            editionId: null,
            now);

        webhookInbox.Add(inbox);
        purchases.Add(purchase);
        await changes.CommitAsync(cancellationToken);

        return ReceiveKiwifyWebhookResult.Accepted(purchaseId, inboxId, alreadyExisted: false);
    }

    private static bool TryParseExternalId(string rawBody, out string externalId)
    {
        externalId = KiwifyWebhookPayloadReader.TryReadString(rawBody, "order_id")
            ?? KiwifyWebhookPayloadReader.TryReadString(rawBody, "orderId")
            ?? KiwifyWebhookPayloadReader.TryReadString(rawBody, "ExternalId")
            ?? KiwifyWebhookPayloadReader.TryReadString(rawBody, "external_id")
            ?? KiwifyWebhookPayloadReader.TryReadString(rawBody, "id")
            ?? string.Empty;
        return !string.IsNullOrWhiteSpace(externalId);
    }
}
