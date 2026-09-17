namespace ContentOS.Contracts.Commerce;

public sealed record KiwifyWebhookAcceptedResponse(
    Guid PurchaseId,
    Guid WebhookInboxEntryId,
    bool AlreadyExisted,
    string StatusHint);
