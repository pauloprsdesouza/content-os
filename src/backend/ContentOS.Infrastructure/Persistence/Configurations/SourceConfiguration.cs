using ContentOS.Domain.Knowledge;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class SourceConfiguration : IEntityTypeConfiguration<Source>
{
    public void Configure(EntityTypeBuilder<Source> builder)
    {
        builder.ToTable("sources", ModuleSchemas.Knowledge);

        builder.HasKey(source => source.Id);

        builder.Property(source => source.DisplayName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(source => source.Kind)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(source => source.CanonicalUri)
            .HasConversion(
                uri => uri.ToString(),
                value => SourceUri.Create(value))
            .HasColumnName("canonical_uri")
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(source => source.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(source => source.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(source => source.CanonicalUri)
            .IsUnique()
            .HasDatabaseName("ux_sources_canonical_uri");

        builder.HasIndex(source => source.UpdatedAt)
            .HasDatabaseName("ix_sources_updated_at");
    }
}
