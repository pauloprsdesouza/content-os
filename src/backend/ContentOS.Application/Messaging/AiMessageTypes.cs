namespace ContentOS.Application.Messaging;

public static class AiMessageTypes
{
    public const string Research = "ai.research";
    public const string ContentGenerate = "ai.content.generate";
    public const string ContentReview = "ai.content.review";
    public const string TopicLabel = "ai.topics.label";

    public const string ResearchCompleted = "ai.research.completed";
    public const string ContentGenerateCompleted = "ai.content.generate.completed";
    public const string ContentReviewCompleted = "ai.content.review.completed";
    public const string TopicLabelCompleted = "ai.topics.label.completed";
    public const string JobFailed = "ai.job.failed";
}
