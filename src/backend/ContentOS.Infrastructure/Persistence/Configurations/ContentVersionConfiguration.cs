using ContentOS.Domain.Content;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class ContentVersionConfiguration : IEntityTypeConfiguration<ContentVersion>
{
    public void Configure(EntityTypeBuilder<ContentVersion> builder)
    {
        builder.ToTable("content_versions", ModuleSchemas.Content);

        builder.HasKey(version => version.Id);

        builder.Property(version => version.ContentUnitId)
            .HasColumnName("content_unit_id")
            .IsRequired();

        builder.Property(version => version.Revision)
            .IsRequired();

        builder.Property(version => version.BodyMarkdown)
            .HasColumnName("body_markdown")
            .HasColumnType("text");

        builder.Property(version => version.Status)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(version => version.Version)
            .IsConcurrencyToken()
            .IsRequired();

        builder.Property(version => version.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.Property(version => version.ReviewedByUserId)
            .HasColumnName("reviewed_by_user_id");

        builder.Property(version => version.ReviewedAt)
            .HasColumnName("reviewed_at");

        builder.Property(version => version.ChangeRequestNotes)
            .HasColumnName("change_request_notes")
            .HasMaxLength(4000);

        builder.Property(version => version.AgentReviewNotes)
            .HasColumnName("agent_review_notes")
            .HasMaxLength(4000);

        builder.Property(version => version.OperationId)
            .HasColumnName("operation_id");

        builder.Property(version => version.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(version => version.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(version => new { version.ContentUnitId, version.Revision })
            .IsUnique()
            .HasDatabaseName("ix_content_versions_unit_revision");

        builder.HasIndex(version => new { version.Status, version.UpdatedAt })
            .HasDatabaseName("ix_content_versions_status_updated_at");
    }
}
