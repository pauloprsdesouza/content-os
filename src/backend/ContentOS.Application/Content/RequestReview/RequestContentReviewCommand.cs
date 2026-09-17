namespace ContentOS.Application.Content.RequestReview;

public sealed record RequestContentReviewCommand(
    Guid ContentVersionId,
    Guid RequestedByUserId);
