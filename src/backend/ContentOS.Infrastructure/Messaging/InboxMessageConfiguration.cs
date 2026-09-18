using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Messaging;

public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages", "wolverine");
        builder.HasKey(message => message.Id);
        builder.Property(message => message.MessageId).HasColumnName("message_id").HasMaxLength(80).IsRequired();
        builder.Property(message => message.EventType).HasColumnName("event_type").HasMaxLength(160).IsRequired();
        builder.Property(message => message.ProcessedAt).HasColumnName("processed_at");
        builder.HasIndex(message => message.MessageId).IsUnique();
    }
}
