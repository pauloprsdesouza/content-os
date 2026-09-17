using ContentOS.Domain.Research;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class ResearchFindingConfiguration : IEntityTypeConfiguration<ResearchFinding>
{
    public void Configure(EntityTypeBuilder<ResearchFinding> builder)
    {
        builder.ToTable("research_findings", ModuleSchemas.Research);

        builder.HasKey(finding => finding.Id);

        builder.Property(finding => finding.ResearchJobId)
            .HasColumnName("research_job_id")
            .IsRequired();

        builder.Property(finding => finding.Statement)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(finding => finding.Confidence)
            .HasPrecision(5, 4)
            .IsRequired();

        builder.Property(finding => finding.CreatedAt)
            .HasColumnName("created_at");

        builder.HasIndex(finding => finding.ResearchJobId)
            .HasDatabaseName("ix_research_findings_job_id");
    }
}
