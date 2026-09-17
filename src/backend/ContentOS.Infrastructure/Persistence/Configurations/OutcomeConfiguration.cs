using ContentOS.Domain.Learning;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class OutcomeConfiguration : IEntityTypeConfiguration<Outcome>
{
    public void Configure(EntityTypeBuilder<Outcome> builder)
    {
        builder.ToTable("outcomes", ModuleSchemas.Learning);

        builder.HasKey(outcome => outcome.Id);

        builder.Property(outcome => outcome.LearnerId)
            .HasColumnName("learner_id")
            .IsRequired();

        builder.Property(outcome => outcome.EnrollmentId)
            .HasColumnName("enrollment_id")
            .IsRequired();

        builder.Property(outcome => outcome.CapstoneId)
            .HasColumnName("capstone_id");

        builder.Property(outcome => outcome.Passed)
            .IsRequired();

        builder.Property(outcome => outcome.RecordedAt)
            .HasColumnName("recorded_at")
            .IsRequired();

        builder.HasIndex(outcome => outcome.EnrollmentId)
            .HasDatabaseName("ix_outcomes_enrollment_id");
    }
}
