using ContentOS.Domain.Catalog;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class CurriculumItemConfiguration : IEntityTypeConfiguration<CurriculumItem>
{
    public void Configure(EntityTypeBuilder<CurriculumItem> builder)
    {
        builder.ToTable("curriculum_items", ModuleSchemas.Catalog);

        builder.HasKey(item => new { item.EditionId, item.Position });

        builder.Property(item => item.EditionId)
            .HasColumnName("edition_id")
            .IsRequired();

        builder.Property(item => item.Position)
            .IsRequired();

        builder.Property(item => item.ContentVersionId)
            .HasColumnName("content_version_id")
            .IsRequired();

        builder.HasIndex(item => new { item.EditionId, item.ContentVersionId })
            .IsUnique()
            .HasDatabaseName("ix_curriculum_items_edition_content_version");
    }
}
