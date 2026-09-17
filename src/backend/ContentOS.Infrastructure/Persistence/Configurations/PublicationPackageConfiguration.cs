using ContentOS.Domain.Publication;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class PublicationPackageConfiguration : IEntityTypeConfiguration<PublicationPackage>
{
    public void Configure(EntityTypeBuilder<PublicationPackage> builder)
    {
        builder.ToTable("publication_packages", ModuleSchemas.Publication);

        builder.HasKey(package => package.Id);

        builder.Property(package => package.EditionId)
            .HasColumnName("edition_id")
            .IsRequired();

        builder.Property(package => package.Status)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(package => package.RendererVersion)
            .HasColumnName("renderer_version")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(package => package.ManifestJson)
            .HasColumnName("manifest_json")
            .HasColumnType("text");

        builder.Property(package => package.ExportBlobSha256)
            .HasColumnName("export_blob_sha256")
            .HasMaxLength(64);

        builder.Property(package => package.ExportBlobLength)
            .HasColumnName("export_blob_length");

        builder.Property(package => package.ConfirmedByUserId)
            .HasColumnName("confirmed_by_user_id");

        builder.Property(package => package.ConfirmedAt)
            .HasColumnName("confirmed_at");

        builder.Property(package => package.Version)
            .IsConcurrencyToken()
            .IsRequired();

        builder.Property(package => package.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(package => package.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(package => new { package.EditionId, package.CreatedAt })
            .HasDatabaseName("ix_publication_packages_edition_created_at");

        builder.HasIndex(package => new { package.Status, package.UpdatedAt })
            .HasDatabaseName("ix_publication_packages_status_updated_at");
    }
}
