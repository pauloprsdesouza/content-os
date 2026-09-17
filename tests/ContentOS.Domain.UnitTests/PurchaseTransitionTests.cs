using ContentOS.Domain.Commerce;
using Shouldly;

namespace ContentOS.Domain.UnitTests;

public sealed class PurchaseTransitionTests
{
    [Fact]
    public void Signal_is_not_confirmed_until_reconciliation()
    {
        var purchase = CreateSignal();

        purchase.Status.ShouldBe(PurchaseStatus.SignalReceived);
        purchase.ConfirmedAt.ShouldBeNull();
        purchase.LearnerId.ShouldBeNull();
    }

    [Fact]
    public void Full_path_to_confirmed_requires_queued_reconciliation()
    {
        var purchase = CreateSignal();
        var now = DateTimeOffset.UtcNow;
        var runId = Guid.CreateVersion7();
        var learnerId = Guid.CreateVersion7();
        var enrollmentId = Guid.CreateVersion7();
        var productId = Guid.CreateVersion7();

        purchase.QueueReconciliation(runId, now);
        purchase.Confirm(
            learnerId,
            enrollmentId,
            productId,
            Guid.CreateVersion7(),
            "buyer@example.com",
            now);

        purchase.Status.ShouldBe(PurchaseStatus.Confirmed);
        purchase.ReconciliationRunId.ShouldBe(runId);
        purchase.LearnerId.ShouldBe(learnerId);
        purchase.EnrollmentId.ShouldBe(enrollmentId);
        purchase.ConfirmedAt.ShouldBe(now);
    }

    [Fact]
    public void Confirm_from_signal_received_fails()
    {
        var purchase = CreateSignal();

        var act = () => purchase.Confirm(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            null,
            "buyer@example.com",
            DateTimeOffset.UtcNow);

        Should.Throw<InvalidOperationException>(act);
        purchase.Status.ShouldBe(PurchaseStatus.SignalReceived);
    }

    [Fact]
    public void Ignore_from_signal_received()
    {
        var purchase = CreateSignal();
        var now = DateTimeOffset.UtcNow;

        purchase.Ignore(now);

        purchase.Status.ShouldBe(PurchaseStatus.Ignored);
        purchase.ConfirmedAt.ShouldBeNull();
    }

    private static Purchase CreateSignal() =>
        Purchase.CreateFromSignal(
            Guid.CreateVersion7(),
            CommerceProviders.Kiwify,
            "order-ext-1",
            Guid.CreateVersion7(),
            "Buyer@Example.com",
            null,
            null,
            DateTimeOffset.UtcNow);
}
