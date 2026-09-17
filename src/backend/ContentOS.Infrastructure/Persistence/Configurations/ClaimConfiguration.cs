using ContentOS.Domain.Knowledge;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("claims", ModuleSchemas.Knowledge);

        builder.HasKey(claim => claim.Id);

        builder.Property(claim => claim.Statement)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(claim => claim.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(claim => claim.Confidence)
            .HasConversion(
                score => score.Value,
                value => ConfidenceScore.Create(value))
            .HasColumnName("confidence")
            .HasPrecision(5, 4)
            .IsRequired();

        builder.Property(claim => claim.Version)
            .IsConcurrencyToken()
            .IsRequired();

        builder.Property(claim => claim.ReviewedByUserId)
            .HasColumnName("reviewed_by_user_id");

        builder.Property(claim => claim.ReviewedAt)
            .HasColumnName("reviewed_at");

        builder.Property(claim => claim.RejectionReason)
            .HasColumnName("rejection_reason")
            .HasMaxLength(2000);

        builder.Property(claim => claim.SupersededByClaimId)
            .HasColumnName("superseded_by_claim_id");

        builder.Property(claim => claim.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(claim => claim.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasMany(claim => claim.EvidenceLinks)
            .WithOne()
            .HasForeignKey(link => link.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(claim => claim.EvidenceLinks)
            .HasField("_evidenceLinks")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(claim => new { claim.Status, claim.UpdatedAt })
            .HasDatabaseName("ix_claims_status_updated_at");
    }
}
