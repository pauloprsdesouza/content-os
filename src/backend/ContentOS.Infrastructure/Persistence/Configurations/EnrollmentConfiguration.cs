using ContentOS.Domain.Learning;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("enrollments", ModuleSchemas.Learning);

        builder.HasKey(enrollment => enrollment.Id);

        builder.Property(enrollment => enrollment.LearnerId)
            .HasColumnName("learner_id")
            .IsRequired();

        builder.Property(enrollment => enrollment.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(enrollment => enrollment.EditionId)
            .HasColumnName("edition_id");

        builder.Property(enrollment => enrollment.PurchaseId)
            .HasColumnName("purchase_id")
            .IsRequired();

        builder.Property(enrollment => enrollment.EnrolledAt)
            .HasColumnName("enrolled_at")
            .IsRequired();

        builder.HasIndex(enrollment => enrollment.PurchaseId)
            .IsUnique()
            .HasDatabaseName("ux_enrollments_purchase_id");

        builder.HasIndex(enrollment => new { enrollment.LearnerId, enrollment.ProductId })
            .HasDatabaseName("ix_enrollments_learner_product");
    }
}
