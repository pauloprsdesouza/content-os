namespace ContentOS.Domain.Learning;

public sealed class Enrollment
{
    private Enrollment(
        Guid id,
        Guid learnerId,
        Guid productId,
        Guid? editionId,
        Guid purchaseId,
        DateTimeOffset enrolledAt)
    {
        Id = id;
        LearnerId = learnerId;
        ProductId = productId;
        EditionId = editionId;
        PurchaseId = purchaseId;
        EnrolledAt = enrolledAt;
    }

    private Enrollment()
    {
    }

    public Guid Id { get; private set; }

    public Guid LearnerId { get; private set; }

    public Guid ProductId { get; private set; }

    public Guid? EditionId { get; private set; }

    public Guid PurchaseId { get; private set; }

    public DateTimeOffset EnrolledAt { get; private set; }

    public static Enrollment Create(
        Guid id,
        Guid learnerId,
        Guid productId,
        Guid? editionId,
        Guid purchaseId,
        DateTimeOffset enrolledAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Enrollment id is required.", nameof(id));
        }

        if (learnerId == Guid.Empty)
        {
            throw new ArgumentException("Learner id is required.", nameof(learnerId));
        }

        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product id is required.", nameof(productId));
        }

        if (purchaseId == Guid.Empty)
        {
            throw new ArgumentException("Purchase id is required.", nameof(purchaseId));
        }

        return new Enrollment(
            id,
            learnerId,
            productId,
            editionId == Guid.Empty ? null : editionId,
            purchaseId,
            enrolledAt);
    }
}
