using ContentOS.Domain.Catalog;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class EditionConfiguration : IEntityTypeConfiguration<Edition>
{
    public void Configure(EntityTypeBuilder<Edition> builder)
    {
        builder.ToTable("editions", ModuleSchemas.Catalog);

        builder.HasKey(edition => edition.Id);

        builder.Property(edition => edition.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(edition => edition.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(edition => edition.Version)
            .IsConcurrencyToken()
            .IsRequired();

        builder.Property(edition => edition.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(edition => edition.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasMany(edition => edition.Curriculum)
            .WithOne()
            .HasForeignKey(item => item.EditionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(edition => edition.Curriculum)
            .HasField("_curriculum")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(edition => new { edition.ProductId, edition.Name })
            .IsUnique()
            .HasDatabaseName("ix_editions_product_name");
    }
}
