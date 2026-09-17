using ContentOS.Domain.Content;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class ContentUnitConfiguration : IEntityTypeConfiguration<ContentUnit>
{
    public void Configure(EntityTypeBuilder<ContentUnit> builder)
    {
        builder.ToTable("content_units", ModuleSchemas.Content);

        builder.HasKey(unit => unit.Id);

        builder.Property(unit => unit.Title)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(unit => unit.Brief)
            .HasMaxLength(4000);

        builder.Property(unit => unit.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.Property(unit => unit.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(unit => unit.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(unit => unit.UpdatedAt)
            .HasDatabaseName("ix_content_units_updated_at");
    }
}
