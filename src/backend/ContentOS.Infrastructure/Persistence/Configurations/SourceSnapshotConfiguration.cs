using ContentOS.Domain.Knowledge;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class SourceSnapshotConfiguration : IEntityTypeConfiguration<SourceSnapshot>
{
    public void Configure(EntityTypeBuilder<SourceSnapshot> builder)
    {
        builder.ToTable("source_snapshots", ModuleSchemas.Knowledge);

        builder.HasKey(snapshot => snapshot.Id);

        builder.Property(snapshot => snapshot.SourceId)
            .HasColumnName("source_id")
            .IsRequired();

        builder.Property(snapshot => snapshot.ContentHash)
            .HasConversion(
                hash => hash.Value,
                value => ContentHash.Create(value))
            .HasColumnName("content_hash")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(snapshot => snapshot.MediaType)
            .HasColumnName("media_type")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(snapshot => snapshot.ByteLength)
            .HasColumnName("byte_length");

        builder.Property(snapshot => snapshot.CapturedAt)
            .HasColumnName("captured_at");

        builder.HasIndex(snapshot => snapshot.ContentHash)
            .HasDatabaseName("ix_source_snapshots_content_hash");

        builder.HasIndex(snapshot => new { snapshot.SourceId, snapshot.CapturedAt })
            .HasDatabaseName("ix_source_snapshots_source_captured");
    }
}
