namespace ContentOS.Domain.Content;

public enum ContentVersionStatus
{
    Draft = 0,
    GenerationQueued = 1,
    Generated = 2,
    ReviewQueued = 3,
    AgentReviewed = 4,
    PendingHumanApproval = 5,
    Approved = 6,
    ChangesRequested = 7
}
