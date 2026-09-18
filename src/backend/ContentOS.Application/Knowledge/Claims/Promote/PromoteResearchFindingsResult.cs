namespace ContentOS.Application.Knowledge.Claims.Promote;

public sealed record PromoteResearchFindingsResult(
    int Promoted,
    int NeedsEvidence,
    int AlreadyPromoted);
