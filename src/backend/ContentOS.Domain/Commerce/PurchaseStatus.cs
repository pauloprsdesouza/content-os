namespace ContentOS.Domain.Commerce;

public enum PurchaseStatus
{
    SignalReceived = 0,
    ReconciliationQueued = 1,
    Confirmed = 2,
    Refunded = 3,
    Chargeback = 4,
    Ignored = 5
}
