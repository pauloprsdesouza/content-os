using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Messaging;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages", "wolverine");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.EnvelopeId)
            .HasColumnName("envelope_id")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(message => message.Type)
            .HasColumnName("type")
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(message => message.Subject)
            .HasColumnName("subject")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(message => message.PayloadJson)
            .HasColumnName("payload_json")
            .IsRequired();

        builder.Property(message => message.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(message => message.SentAt)
            .HasColumnName("sent_at");

        builder.Property(message => message.AttemptCount)
            .HasColumnName("attempt_count");

        builder.Property(message => message.LastError)
            .HasColumnName("last_error")
            .HasMaxLength(500);

        builder.HasIndex(message => message.EnvelopeId)
            .IsUnique();

        builder.HasIndex(message => message.SentAt);
    }
}
