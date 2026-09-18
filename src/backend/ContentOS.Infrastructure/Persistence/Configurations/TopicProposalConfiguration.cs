using ContentOS.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentOS.Infrastructure.Persistence.Configurations;

public sealed class TopicProposalConfiguration : IEntityTypeConfiguration<TopicProposal>
{
    public void Configure(EntityTypeBuilder<TopicProposal> builder)
    {
        builder.ToTable("topic_proposals", ModuleSchemas.Content);
        builder.HasKey(proposal => proposal.Id);
        builder.Property(proposal => proposal.DiscoveryId).HasColumnName("discovery_id");
        builder.Property(proposal => proposal.Label).HasMaxLength(300).IsRequired();
        builder.Property(proposal => proposal.Rationale).HasMaxLength(2000);
        builder.Property(proposal => proposal.WorkIds).HasColumnName("work_ids").HasMaxLength(2000).IsRequired();
        builder.Property(proposal => proposal.IsSelected).HasColumnName("is_selected");
        builder.HasIndex(proposal => proposal.DiscoveryId);
    }
}
