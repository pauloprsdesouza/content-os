using ContentOS.Domain.Learning;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class LearnerConfiguration : IEntityTypeConfiguration<Learner>
{
    public void Configure(EntityTypeBuilder<Learner> builder)
    {
        builder.ToTable("learners", ModuleSchemas.Learning);

        builder.HasKey(learner => learner.Id);

        builder.Property(learner => learner.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(learner => learner.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(256);

        builder.Property(learner => learner.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(learner => learner.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(learner => learner.Email)
            .IsUnique()
            .HasDatabaseName("ux_learners_email");
    }
}
