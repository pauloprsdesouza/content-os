using ContentOS.Domain.Content;
using Shouldly;

namespace ContentOS.Domain.UnitTests;

public sealed class ContentVersionTransitionTests
{
    [Fact]
    public void Full_generation_path_to_approved()
    {
        var version = CreateDraft();
        var now = DateTimeOffset.UtcNow;
        var actor = Guid.CreateVersion7();

        version.QueueGeneration(now);
        version.MarkGenerated("# Hello", now);
        version.QueueAgentReview(now);
        version.MarkAgentReviewed("looks good", now);
        version.RequestHumanApproval(now);
        version.Approve(actor, version.Version, now);

        version.Status.ShouldBe(ContentVersionStatus.Approved);
        version.ReviewedByUserId.ShouldBe(actor);
        version.BodyMarkdown.ShouldBe("# Hello");
    }

    [Fact]
    public void RequestHumanApproval_can_skip_from_generated()
    {
        var version = CreateDraft();
        var now = DateTimeOffset.UtcNow;
        version.QueueGeneration(now);
        version.MarkGenerated("Body", now);

        version.RequestHumanApproval(now);

        version.Status.ShouldBe(ContentVersionStatus.PendingHumanApproval);
    }

    [Fact]
    public void Approved_body_cannot_be_edited()
    {
        var version = CreateDraft();
        var now = DateTimeOffset.UtcNow;
        version.UpdateDraftBody("editable", version.Version, now);
        version.RequestHumanApproval(now);
        version.Approve(Guid.CreateVersion7(), version.Version, now);

        var act = () => version.UpdateDraftBody("nope", version.Version, now);

        Should.Throw<InvalidOperationException>(act);
        version.Status.ShouldBe(ContentVersionStatus.Approved);
    }

    [Fact]
    public void RequestChanges_returns_to_changes_requested()
    {
        var version = CreateDraft();
        var now = DateTimeOffset.UtcNow;
        version.UpdateDraftBody("draft body", version.Version, now);
        version.RequestHumanApproval(now);
        var expected = version.Version;

        version.RequestChanges(
            Guid.CreateVersion7(),
            "Need stronger CTA",
            expected,
            now);

        version.Status.ShouldBe(ContentVersionStatus.ChangesRequested);
        version.ChangeRequestNotes.ShouldBe("Need stronger CTA");
        version.Version.ShouldBe(expected + 1);
    }

    [Fact]
    public void Approve_with_stale_version_fails()
    {
        var version = CreateDraft();
        var now = DateTimeOffset.UtcNow;
        version.UpdateDraftBody("body", version.Version, now);
        version.RequestHumanApproval(now);

        var act = () => version.Approve(
            Guid.CreateVersion7(),
            version.Version + 1,
            now);

        Should.Throw<InvalidOperationException>(act);
        version.Status.ShouldBe(ContentVersionStatus.PendingHumanApproval);
    }

    [Fact]
    public void ChangesRequested_can_queue_generation_again()
    {
        var version = CreateDraft();
        var now = DateTimeOffset.UtcNow;
        version.UpdateDraftBody("body", version.Version, now);
        version.RequestHumanApproval(now);
        version.RequestChanges(Guid.CreateVersion7(), "fix tone", version.Version, now);

        version.QueueGeneration(now);

        version.Status.ShouldBe(ContentVersionStatus.GenerationQueued);
    }

    private static ContentVersion CreateDraft() =>
        ContentVersion.CreateDraft(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            revision: 1,
            bodyMarkdown: null,
            createdByUserId: Guid.CreateVersion7(),
            createdAt: DateTimeOffset.UtcNow);
}
