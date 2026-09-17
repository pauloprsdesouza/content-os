using ContentOS.Domain.Catalog;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products", ModuleSchemas.Catalog);

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(product => product.Description)
            .HasMaxLength(2000);

        builder.Property(product => product.Version)
            .IsConcurrencyToken()
            .IsRequired();

        builder.Property(product => product.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(product => product.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(product => product.Name)
            .HasDatabaseName("ix_products_name");
    }
}
