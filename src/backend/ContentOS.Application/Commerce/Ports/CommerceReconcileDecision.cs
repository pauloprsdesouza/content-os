namespace ContentOS.Application.Commerce.Ports;

public enum CommerceReconcileDecision
{
    Confirm = 0,
    Refund = 1,
    Chargeback = 2,
    Ignore = 3
}
