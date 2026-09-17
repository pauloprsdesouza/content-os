using ContentOS.Domain.Knowledge;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class ClaimEvidenceLinkConfiguration : IEntityTypeConfiguration<ClaimEvidenceLink>
{
    public void Configure(EntityTypeBuilder<ClaimEvidenceLink> builder)
    {
        builder.ToTable("claim_evidence", ModuleSchemas.Knowledge);

        builder.HasKey(link => new { link.ClaimId, link.EvidenceId });

        builder.Property(link => link.ClaimId)
            .HasColumnName("claim_id");

        builder.Property(link => link.EvidenceId)
            .HasColumnName("evidence_id");

        builder.Property(link => link.LinkedAt)
            .HasColumnName("linked_at");

        builder.HasIndex(link => link.EvidenceId)
            .HasDatabaseName("ix_claim_evidence_evidence_id");
    }
}
