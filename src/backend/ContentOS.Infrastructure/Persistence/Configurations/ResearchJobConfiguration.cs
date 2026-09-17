using ContentOS.Domain.Research;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class ResearchJobConfiguration : IEntityTypeConfiguration<ResearchJob>
{
    public void Configure(EntityTypeBuilder<ResearchJob> builder)
    {
        builder.ToTable("research_jobs", ModuleSchemas.Research);

        builder.HasKey(job => job.Id);

        builder.Property(job => job.Topic)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(job => job.ScopeNotes)
            .HasColumnName("scope_notes")
            .HasMaxLength(4000);

        builder.Property(job => job.RequestedByUserId)
            .HasColumnName("requested_by_user_id")
            .IsRequired();

        builder.Property(job => job.Status)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(job => job.Version)
            .IsConcurrencyToken()
            .IsRequired();

        builder.Property(job => job.AttemptCount)
            .HasColumnName("attempt_count")
            .IsRequired();

        builder.Property(job => job.FailureReason)
            .HasColumnName("failure_reason")
            .HasMaxLength(2000);

        builder.Property(job => job.OperationId)
            .HasColumnName("operation_id");

        builder.Property(job => job.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(job => job.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasMany(job => job.Findings)
            .WithOne()
            .HasForeignKey(finding => finding.ResearchJobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(job => job.Findings)
            .HasField("_findings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(job => new { job.Status, job.UpdatedAt })
            .HasDatabaseName("ix_research_jobs_status_updated_at");
    }
}
