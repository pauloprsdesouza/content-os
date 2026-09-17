namespace ContentOS.Application.Commerce.Ports;

public sealed record CommerceOrderSnapshot(
    string ExternalId,
    CommerceReconcileDecision Decision,
    string BuyerEmail,
    string? BuyerDisplayName,
    Guid? ProductId,
    Guid? EditionId);
