using ContentOS.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class EditorialSeriesConfiguration : IEntityTypeConfiguration<EditorialSeries>
{
    public void Configure(EntityTypeBuilder<EditorialSeries> builder)
    {
        builder.ToTable("editorial_series", ModuleSchemas.Content);
        builder.HasKey(series => series.Id);
        builder.Property(series => series.Format)
            .HasConversion(format => format.Code, code => ContentFormat.Parse(code))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(series => series.AreaId).HasColumnName("area_id").HasMaxLength(64).IsRequired();
        builder.Property(series => series.AreaName).HasColumnName("area_name").HasMaxLength(200).IsRequired();
        builder.Property(series => series.AreaIsSubfield).HasColumnName("area_is_subfield");
        builder.Property(series => series.WindowDays).HasColumnName("window_days");
        builder.Property(series => series.Cadence).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(series => series.OwnerUserId).HasColumnName("owner_user_id");
        builder.Property(series => series.NextCollectionAt).HasColumnName("next_collection_at");
        builder.Property(series => series.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(series => series.OwnerUserId);
    }
}
