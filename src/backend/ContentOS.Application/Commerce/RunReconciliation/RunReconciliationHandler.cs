using ContentOS.Application.Commerce.Ports;
using ContentOS.Application.Learning.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Commerce;
using ContentOS.Domain.Learning;
using ContentOS.SharedKernel;

namespace ContentOS.Application.Commerce.RunReconciliation;

public sealed class RunReconciliationHandler(
    IPurchaseRepository purchases,
    IWebhookInboxRepository webhookInbox,
    IReconciliationRunRepository reconciliationRuns,
    ICommerceOrderReconciler orderReconciler,
    ILearnerRepository learners,
    IEnrollmentRepository enrollments,
    ICapstoneRepository capstones,
    IIdGenerator ids,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<RunReconciliationResult> HandleAsync(
        RunReconciliationCommand command,
        Guid? defaultProductId,
        Guid? defaultEditionId,
        CancellationToken cancellationToken = default)
    {
        _ = command;
        var pending = await purchases.ListPendingReconciliationAsync(cancellationToken);
        var now = clock.GetUtcNow();
        var run = ReconciliationRun.Start(ids.NewId(), now);
        reconciliationRuns.Add(run);

        try
        {
            foreach (var purchase in pending)
            {
                string? payload = null;
                if (purchase.WebhookInboxEntryId is Guid inboxId)
                {
                    var inbox = await webhookInbox.GetByIdAsync(inboxId, cancellationToken);
                    payload = inbox?.PayloadJson;
                }

                CommerceOrderSnapshot snapshot;
                try
                {
                    snapshot = await orderReconciler.GetAuthoritativeOrderAsync(
                        purchase.Provider,
                        purchase.ExternalId,
                        payload,
                        cancellationToken);
                }
                catch (CommerceProviderNotConfiguredException ex)
                {
                    run.Fail(clock.GetUtcNow(), ex.Message);
                    await changes.CommitAsync(cancellationToken);
                    return RunReconciliationResult.Failed(
                        "COMMERCE_PROVIDER_NOT_CONFIGURED",
                        ex.Message);
                }

                purchase.QueueReconciliation(run.Id, now);

                switch (snapshot.Decision)
                {
                    case CommerceReconcileDecision.Confirm:
                        await ConfirmPurchaseAsync(
                            purchase,
                            snapshot,
                            defaultProductId,
                            defaultEditionId,
                            now,
                            cancellationToken);
                        run.RecordConfirmed();
                        break;
                    case CommerceReconcileDecision.Refund:
                        purchase.MarkRefunded(now);
                        run.RecordIgnored();
                        break;
                    case CommerceReconcileDecision.Chargeback:
                        purchase.MarkChargeback(now);
                        run.RecordIgnored();
                        break;
                    default:
                        purchase.Ignore(now);
                        run.RecordIgnored();
                        break;
                }
            }

            run.Complete(clock.GetUtcNow(), $"Processed {run.ProcessedCount} purchase(s).");
            await changes.CommitAsync(cancellationToken);
            return RunReconciliationResult.Completed(
                run.Id,
                run.ProcessedCount,
                run.ConfirmedCount,
                run.IgnoredCount);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            run.Fail(clock.GetUtcNow(), ex.Message);
            await changes.CommitAsync(cancellationToken);
            return RunReconciliationResult.Failed("COMMERCE_RECONCILIATION_FAILED", ex.Message);
        }
    }

    private async Task ConfirmPurchaseAsync(
        Purchase purchase,
        CommerceOrderSnapshot snapshot,
        Guid? defaultProductId,
        Guid? defaultEditionId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var productId = snapshot.ProductId
            ?? purchase.ProductId
            ?? defaultProductId
            ?? throw new InvalidOperationException(
                "Confirmed purchase requires a product id (payload or Commerce:DefaultProductId).");
        var editionId = snapshot.EditionId ?? purchase.EditionId ?? defaultEditionId;
        var email = snapshot.BuyerEmail;
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException(
                $"Purchase {purchase.ExternalId} is missing buyer email for confirmation.");
        }

        var learner = await learners.GetByEmailAsync(email, cancellationToken);
        if (learner is null)
        {
            learner = Learner.Create(ids.NewId(), email, snapshot.BuyerDisplayName, now);
            learners.Add(learner);
        }

        var enrollment = Enrollment.Create(
            ids.NewId(),
            learner.Id,
            productId,
            editionId,
            purchase.Id,
            now);
        enrollments.Add(enrollment);

        var capstone = Capstone.Create(
            ids.NewId(),
            enrollment.Id,
            "Capstone MVP",
            now);
        capstones.Add(capstone);

        purchase.Confirm(
            learner.Id,
            enrollment.Id,
            productId,
            editionId,
            email,
            now);
    }
}
