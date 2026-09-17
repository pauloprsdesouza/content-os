using ContentOS.Domain.Commerce;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class WebhookInboxEntryConfiguration : IEntityTypeConfiguration<WebhookInboxEntry>
{
    public void Configure(EntityTypeBuilder<WebhookInboxEntry> builder)
    {
        builder.ToTable("webhook_inbox", ModuleSchemas.Commerce);

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Provider)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(entry => entry.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(entry => entry.PayloadJson)
            .HasColumnName("payload_json")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(entry => entry.SignatureVerified)
            .HasColumnName("signature_verified")
            .IsRequired();

        builder.Property(entry => entry.ReceivedAt)
            .HasColumnName("received_at")
            .IsRequired();

        builder.HasIndex(entry => new { entry.Provider, entry.ExternalId })
            .IsUnique()
            .HasDatabaseName("ux_webhook_inbox_provider_external_id");
    }
}
