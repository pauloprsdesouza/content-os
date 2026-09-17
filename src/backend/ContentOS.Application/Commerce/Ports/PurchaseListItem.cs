namespace ContentOS.Application.Commerce.Ports;

public sealed record PurchaseListItem(
    Guid Id,
    string Provider,
    string ExternalId,
    string Status,
    string? BuyerEmail,
    Guid? ProductId,
    Guid? EditionId,
    Guid? LearnerId,
    Guid? WebhookInboxEntryId,
    Guid? ReconciliationRunId,
    DateTimeOffset? ConfirmedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
