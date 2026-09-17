using ContentOS.Domain.Commerce;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class ReconciliationRunConfiguration : IEntityTypeConfiguration<ReconciliationRun>
{
    public void Configure(EntityTypeBuilder<ReconciliationRun> builder)
    {
        builder.ToTable("reconciliation_runs", ModuleSchemas.Commerce);

        builder.HasKey(run => run.Id);

        builder.Property(run => run.Status)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(run => run.StartedAt)
            .HasColumnName("started_at")
            .IsRequired();

        builder.Property(run => run.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(run => run.ProcessedCount)
            .HasColumnName("processed_count")
            .IsRequired();

        builder.Property(run => run.ConfirmedCount)
            .HasColumnName("confirmed_count")
            .IsRequired();

        builder.Property(run => run.IgnoredCount)
            .HasColumnName("ignored_count")
            .IsRequired();

        builder.Property(run => run.Notes)
            .HasMaxLength(1024);
    }
}
