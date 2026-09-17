namespace ContentOS.Domain.Commerce;

public sealed class Purchase
{
    private Purchase(
        Guid id,
        string provider,
        string externalId,
        Guid? webhookInboxEntryId,
        string? buyerEmail,
        Guid? productId,
        Guid? editionId,
        DateTimeOffset createdAt)
    {
        Id = id;
        Provider = provider;
        ExternalId = externalId;
        WebhookInboxEntryId = webhookInboxEntryId;
        BuyerEmail = buyerEmail;
        ProductId = productId;
        EditionId = editionId;
        Status = PurchaseStatus.SignalReceived;
        Version = 1;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private Purchase()
    {
        Provider = null!;
        ExternalId = null!;
    }

    public Guid Id { get; private set; }

    public string Provider { get; private set; }

    public string ExternalId { get; private set; }

    public PurchaseStatus Status { get; private set; }

    public Guid? WebhookInboxEntryId { get; private set; }

    public Guid? ReconciliationRunId { get; private set; }

    public string? BuyerEmail { get; private set; }

    public Guid? ProductId { get; private set; }

    public Guid? EditionId { get; private set; }

    public Guid? LearnerId { get; private set; }

    public Guid? EnrollmentId { get; private set; }

    public DateTimeOffset? ConfirmedAt { get; private set; }

    public long Version { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Purchase CreateFromSignal(
        Guid id,
        string provider,
        string externalId,
        Guid? webhookInboxEntryId,
        string? buyerEmail,
        Guid? productId,
        Guid? editionId,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Purchase id is required.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId);

        return new Purchase(
            id,
            provider.Trim().ToLowerInvariant(),
            externalId.Trim(),
            webhookInboxEntryId,
            NormalizeEmail(buyerEmail),
            productId == Guid.Empty ? null : productId,
            editionId == Guid.Empty ? null : editionId,
            createdAt);
    }

    public void QueueReconciliation(Guid reconciliationRunId, DateTimeOffset at)
    {
        if (reconciliationRunId == Guid.Empty)
        {
            throw new ArgumentException("Reconciliation run id is required.", nameof(reconciliationRunId));
        }

        EnsureStatus(PurchaseStatus.SignalReceived, "queue reconciliation");

        ReconciliationRunId = reconciliationRunId;
        Status = PurchaseStatus.ReconciliationQueued;
        Version++;
        Touch(at);
    }

    public void Confirm(
        Guid learnerId,
        Guid enrollmentId,
        Guid productId,
        Guid? editionId,
        string buyerEmail,
        DateTimeOffset confirmedAt)
    {
        if (learnerId == Guid.Empty)
        {
            throw new ArgumentException("Learner id is required.", nameof(learnerId));
        }

        if (enrollmentId == Guid.Empty)
        {
            throw new ArgumentException("Enrollment id is required.", nameof(enrollmentId));
        }

        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product id is required.", nameof(productId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(buyerEmail);
        EnsureStatus(PurchaseStatus.ReconciliationQueued, "confirm");

        LearnerId = learnerId;
        EnrollmentId = enrollmentId;
        ProductId = productId;
        EditionId = editionId == Guid.Empty ? null : editionId;
        BuyerEmail = NormalizeEmail(buyerEmail)!;
        ConfirmedAt = confirmedAt;
        Status = PurchaseStatus.Confirmed;
        Version++;
        Touch(confirmedAt);
    }

    public void MarkRefunded(DateTimeOffset at)
    {
        EnsureTerminalSource("mark refunded");
        Status = PurchaseStatus.Refunded;
        Version++;
        Touch(at);
    }

    public void MarkChargeback(DateTimeOffset at)
    {
        EnsureTerminalSource("mark chargeback");
        Status = PurchaseStatus.Chargeback;
        Version++;
        Touch(at);
    }

    public void Ignore(DateTimeOffset at)
    {
        EnsureTerminalSource("ignore");
        Status = PurchaseStatus.Ignored;
        Version++;
        Touch(at);
    }

    private void EnsureStatus(PurchaseStatus expected, string action)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException(
                $"Purchase {Id} cannot {action} from {Status}.");
        }
    }

    private void EnsureTerminalSource(string action)
    {
        if (Status is not (PurchaseStatus.SignalReceived
            or PurchaseStatus.ReconciliationQueued
            or PurchaseStatus.Confirmed))
        {
            throw new InvalidOperationException(
                $"Purchase {Id} cannot {action} from {Status}.");
        }
    }

    private static string? NormalizeEmail(string? email) =>
        string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

    private void Touch(DateTimeOffset at) => UpdatedAt = at;
}
