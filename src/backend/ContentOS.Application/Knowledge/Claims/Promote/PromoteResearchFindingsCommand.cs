namespace ContentOS.Application.Knowledge.Claims.Promote;

public sealed record PromoteResearchFindingsCommand(
    Guid ResearchJobId,
    IReadOnlyList<PromoteResearchFindingItem> Findings);
