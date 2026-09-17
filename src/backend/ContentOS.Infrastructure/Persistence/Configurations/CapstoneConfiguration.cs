using ContentOS.Domain.Learning;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class CapstoneConfiguration : IEntityTypeConfiguration<Capstone>
{
    public void Configure(EntityTypeBuilder<Capstone> builder)
    {
        builder.ToTable("capstones", ModuleSchemas.Learning);

        builder.HasKey(capstone => capstone.Id);

        builder.Property(capstone => capstone.EnrollmentId)
            .HasColumnName("enrollment_id")
            .IsRequired();

        builder.Property(capstone => capstone.Title)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(capstone => capstone.Status)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(capstone => capstone.SubmittedAt)
            .HasColumnName("submitted_at");

        builder.Property(capstone => capstone.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(capstone => capstone.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(capstone => capstone.EnrollmentId)
            .IsUnique()
            .HasDatabaseName("ux_capstones_enrollment_id");
    }
}
