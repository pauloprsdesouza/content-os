using ContentOS.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class TopicDiscoveryConfiguration : IEntityTypeConfiguration<TopicDiscovery>
{
    public void Configure(EntityTypeBuilder<TopicDiscovery> builder)
    {
        builder.ToTable("topic_discoveries", ModuleSchemas.Content);
        builder.HasKey(discovery => discovery.Id);
        builder.Property(discovery => discovery.Format)
            .HasConversion(format => format.Code, code => ContentFormat.Parse(code))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(discovery => discovery.AreaId).HasColumnName("area_id").HasMaxLength(64).IsRequired();
        builder.Property(discovery => discovery.AreaName).HasColumnName("area_name").HasMaxLength(200).IsRequired();
        builder.Property(discovery => discovery.AreaIsSubfield).HasColumnName("area_is_subfield");
        builder.Property(discovery => discovery.WindowDays).HasColumnName("window_days");
        builder.Property(discovery => discovery.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(discovery => discovery.OwnerUserId).HasColumnName("owner_user_id");
        builder.Property(discovery => discovery.SeriesId).HasColumnName("series_id");
        builder.Property(discovery => discovery.ScheduledFor).HasColumnName("scheduled_for");
        builder.Property(discovery => discovery.OperationId).HasColumnName("operation_id");
        builder.Property(discovery => discovery.ContentUnitId).HasColumnName("content_unit_id");
        builder.Property(discovery => discovery.CreatedAt).HasColumnName("created_at");
        builder.Property(discovery => discovery.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(discovery => new { discovery.OwnerUserId, discovery.UpdatedAt });
        builder.HasIndex(discovery => new { discovery.SeriesId, discovery.ScheduledFor })
            .IsUnique()
            .HasFilter("series_id IS NOT NULL AND scheduled_for IS NOT NULL");
    }
}
