using ContentOS.Domain.Knowledge;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class EvidenceConfiguration : IEntityTypeConfiguration<Evidence>
{
    public void Configure(EntityTypeBuilder<Evidence> builder)
    {
        builder.ToTable("evidence", ModuleSchemas.Knowledge);

        builder.HasKey(evidence => evidence.Id);

        builder.Property(evidence => evidence.SnapshotId)
            .HasColumnName("snapshot_id")
            .IsRequired();

        builder.Property(evidence => evidence.Locator)
            .HasConversion(
                locator => locator.Value,
                value => EvidenceLocator.Create(value))
            .HasColumnName("locator")
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(evidence => evidence.ExtractionMethod)
            .HasColumnName("extraction_method")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(evidence => evidence.Confidence)
            .HasConversion(
                score => score.Value,
                value => ConfidenceScore.Create(value))
            .HasColumnName("confidence")
            .HasPrecision(5, 4)
            .IsRequired();

        builder.Property(evidence => evidence.CreatedAt)
            .HasColumnName("created_at");

        builder.HasIndex(evidence => evidence.SnapshotId)
            .HasDatabaseName("ix_evidence_snapshot_id");
    }
}
