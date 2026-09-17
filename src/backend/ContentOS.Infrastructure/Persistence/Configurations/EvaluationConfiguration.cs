using ContentOS.Domain.Learning;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class EvaluationConfiguration : IEntityTypeConfiguration<Evaluation>
{
    public void Configure(EntityTypeBuilder<Evaluation> builder)
    {
        builder.ToTable("evaluations", ModuleSchemas.Learning);

        builder.HasKey(evaluation => evaluation.Id);

        builder.Property(evaluation => evaluation.CapstoneId)
            .HasColumnName("capstone_id")
            .IsRequired();

        builder.Property(evaluation => evaluation.Passed)
            .IsRequired();

        builder.Property(evaluation => evaluation.Score);

        builder.Property(evaluation => evaluation.EvaluatedAt)
            .HasColumnName("evaluated_at")
            .IsRequired();

        builder.HasIndex(evaluation => evaluation.CapstoneId)
            .HasDatabaseName("ix_evaluations_capstone_id");
    }
}
