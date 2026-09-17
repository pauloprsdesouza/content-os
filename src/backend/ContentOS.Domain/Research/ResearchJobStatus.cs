namespace ContentOS.Domain.Research;

public enum ResearchJobStatus
{
    Requested = 0,
    Queued = 1,
    Running = 2,
    AwaitingKnowledgeReview = 3,
    Completed = 4,
    RetryScheduled = 5,
    Failed = 6,
    Cancelled = 7
}
