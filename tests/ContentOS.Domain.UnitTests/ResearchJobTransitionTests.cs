using ContentOS.Domain.Research;
using Shouldly;

namespace ContentOS.Domain.UnitTests;

public sealed class ResearchJobTransitionTests
{
    [Fact]
    public void Happy_path_reaches_completed()
    {
        var job = CreateRequested();
        var now = DateTimeOffset.UtcNow;

        job.MarkQueued(now);
        job.MarkRunning(now);
        job.MarkAwaitingKnowledgeReview(
            [
                ResearchFinding.Create(
                    Guid.CreateVersion7(),
                    job.Id,
                    "Finding A",
                    0.9m,
                    now)
            ],
            now);
        job.MarkCompleted(now);

        job.Status.ShouldBe(ResearchJobStatus.Completed);
        job.Findings.Count.ShouldBe(1);
        job.AttemptCount.ShouldBe(1);
        job.Version.ShouldBe(5);
    }

    [Fact]
    public void MarkRunning_from_requested_fails()
    {
        var job = CreateRequested();

        var act = () => job.MarkRunning(DateTimeOffset.UtcNow);

        Should.Throw<InvalidOperationException>(act);
        job.Status.ShouldBe(ResearchJobStatus.Requested);
    }

    [Fact]
    public void Awaiting_review_requires_findings()
    {
        var job = CreateRequested();
        var now = DateTimeOffset.UtcNow;
        job.MarkQueued(now);
        job.MarkRunning(now);

        var act = () => job.MarkAwaitingKnowledgeReview([], now);

        Should.Throw<InvalidOperationException>(act);
        job.Status.ShouldBe(ResearchJobStatus.Running);
    }

    [Fact]
    public void ScheduleRetry_then_run_again()
    {
        var job = CreateRequested();
        var now = DateTimeOffset.UtcNow;
        job.MarkQueued(now);
        job.MarkRunning(now);
        job.ScheduleRetry("transient", now);

        job.Status.ShouldBe(ResearchJobStatus.RetryScheduled);
        job.FailureReason.ShouldBe("transient");

        job.MarkRunning(now);
        job.AttemptCount.ShouldBe(2);
        job.Status.ShouldBe(ResearchJobStatus.Running);
    }

    [Fact]
    public void Cancel_from_completed_fails()
    {
        var job = CreateRequested();
        var now = DateTimeOffset.UtcNow;
        job.MarkQueued(now);
        job.MarkRunning(now);
        job.MarkAwaitingKnowledgeReview(
            [
                ResearchFinding.Create(
                    Guid.CreateVersion7(),
                    job.Id,
                    "Done",
                    0.8m,
                    now)
            ],
            now);
        job.MarkCompleted(now);

        var act = () => job.Cancel(null, now);

        Should.Throw<InvalidOperationException>(act);
    }

    private static ResearchJob CreateRequested() =>
        ResearchJob.Create(
            Guid.CreateVersion7(),
            "MVP research topic",
            "scope notes",
            Guid.CreateVersion7(),
            DateTimeOffset.UtcNow);
}
