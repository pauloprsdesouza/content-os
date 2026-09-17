using ContentOS.Domain.Operations;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class OperationConfiguration : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        // Assumption: platform Operation lives in identity schema for MVP.
        builder.ToTable("operations", ModuleSchemas.Identity);

        builder.HasKey(operation => operation.Id);

        builder.Property(operation => operation.Kind)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(operation => operation.SubjectType)
            .HasColumnName("subject_type")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(operation => operation.SubjectId)
            .HasColumnName("subject_id")
            .IsRequired();

        builder.Property(operation => operation.RequestedByUserId)
            .HasColumnName("requested_by_user_id")
            .IsRequired();

        builder.Property(operation => operation.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(operation => operation.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        builder.Property(operation => operation.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(operation => operation.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(operation => new { operation.SubjectType, operation.SubjectId })
            .HasDatabaseName("ix_operations_subject");
    }
}
