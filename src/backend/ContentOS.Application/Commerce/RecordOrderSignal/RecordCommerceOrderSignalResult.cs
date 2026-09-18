namespace ContentOS.Application.Commerce.RecordOrderSignal;

public sealed record RecordCommerceOrderSignalResult(
    bool IsSuccess,
    Guid? PurchaseId,
    bool AlreadyExisted,
    string? ErrorCode)
{
    public static RecordCommerceOrderSignalResult Accepted(Guid purchaseId, bool alreadyExisted) =>
        new(true, purchaseId, alreadyExisted, null);

    public static RecordCommerceOrderSignalResult Invalid(string errorCode) =>
        new(false, null, false, errorCode);
}
