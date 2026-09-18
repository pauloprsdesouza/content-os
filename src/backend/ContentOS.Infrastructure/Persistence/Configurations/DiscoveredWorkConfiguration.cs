using ContentOS.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class DiscoveredWorkConfiguration : IEntityTypeConfiguration<DiscoveredWork>
{
    public void Configure(EntityTypeBuilder<DiscoveredWork> builder)
    {
        builder.ToTable("discovered_works", ModuleSchemas.Content);
        builder.HasKey(work => work.Id);
        builder.Property(work => work.DiscoveryId).HasColumnName("discovery_id");
        builder.Property(work => work.WorkId).HasColumnName("work_id").HasMaxLength(32).IsRequired();
        builder.Property(work => work.Title).HasMaxLength(500).IsRequired();
        builder.Property(work => work.PublicationYear).HasColumnName("publication_year");
        builder.Property(work => work.TopicId).HasColumnName("topic_id").HasMaxLength(32);
        builder.Property(work => work.TopicName).HasColumnName("topic_name").HasMaxLength(200);
        builder.Property(work => work.AbstractText).HasColumnName("abstract_text").HasColumnType("text");
        builder.HasIndex(work => work.DiscoveryId);
    }
}
