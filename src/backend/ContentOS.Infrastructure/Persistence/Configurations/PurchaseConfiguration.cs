using ContentOS.Domain.Commerce;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.ToTable("purchases", ModuleSchemas.Commerce);

        builder.HasKey(purchase => purchase.Id);

        builder.Property(purchase => purchase.Provider)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(purchase => purchase.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(purchase => purchase.Status)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(purchase => purchase.WebhookInboxEntryId)
            .HasColumnName("webhook_inbox_entry_id");

        builder.Property(purchase => purchase.ReconciliationRunId)
            .HasColumnName("reconciliation_run_id");

        builder.Property(purchase => purchase.BuyerEmail)
            .HasColumnName("buyer_email")
            .HasMaxLength(320);

        builder.Property(purchase => purchase.ProductId)
            .HasColumnName("product_id");

        builder.Property(purchase => purchase.EditionId)
            .HasColumnName("edition_id");

        builder.Property(purchase => purchase.LearnerId)
            .HasColumnName("learner_id");

        builder.Property(purchase => purchase.EnrollmentId)
            .HasColumnName("enrollment_id");

        builder.Property(purchase => purchase.ConfirmedAt)
            .HasColumnName("confirmed_at");

        builder.Property(purchase => purchase.Version)
            .IsConcurrencyToken()
            .IsRequired();

        builder.Property(purchase => purchase.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(purchase => purchase.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(purchase => new { purchase.Provider, purchase.ExternalId })
            .IsUnique()
            .HasDatabaseName("ux_purchases_provider_external_id");

        builder.HasIndex(purchase => new { purchase.Status, purchase.CreatedAt })
            .HasDatabaseName("ix_purchases_status_created_at");
    }
}
