namespace ContentOS.Application.Commerce.ReceiveKiwifyWebhook;

public sealed record ReceiveKiwifyWebhookResult
{
    private ReceiveKiwifyWebhookResult(
        bool isSuccess,
        Guid? purchaseId,
        Guid? webhookInboxEntryId,
        bool alreadyExisted,
        string? errorCode)
    {
        IsSuccess = isSuccess;
        PurchaseId = purchaseId;
        WebhookInboxEntryId = webhookInboxEntryId;
        AlreadyExisted = alreadyExisted;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }

    public Guid? PurchaseId { get; }

    public Guid? WebhookInboxEntryId { get; }

    public bool AlreadyExisted { get; }

    public string? ErrorCode { get; }

    public static ReceiveKiwifyWebhookResult Accepted(
        Guid purchaseId,
        Guid webhookInboxEntryId,
        bool alreadyExisted) =>
        new(true, purchaseId, webhookInboxEntryId, alreadyExisted, null);

    public static ReceiveKiwifyWebhookResult InvalidSignature() =>
        new(false, null, null, false, "COMMERCE_WEBHOOK_INVALID_SIGNATURE");

    public static ReceiveKiwifyWebhookResult InvalidPayload(string code) =>
        new(false, null, null, false, code);
}
