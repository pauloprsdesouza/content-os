using ContentOS.Domain.Learning;
using Shouldly;

namespace ContentOS.Domain.UnitTests;

public sealed class CapstoneTransitionTests
{
    [Fact]
    public void Submit_then_evaluate()
    {
        var capstone = Capstone.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "MVP Capstone",
            DateTimeOffset.UtcNow);
        var now = DateTimeOffset.UtcNow;

        capstone.Submit(now);
        capstone.MarkEvaluated(now.AddMinutes(1));

        capstone.Status.ShouldBe(CapstoneStatus.Evaluated);
        capstone.SubmittedAt.ShouldBe(now);
    }

    [Fact]
    public void Evaluate_from_open_fails()
    {
        var capstone = Capstone.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "MVP Capstone",
            DateTimeOffset.UtcNow);

        Should.Throw<InvalidOperationException>(() =>
            capstone.MarkEvaluated(DateTimeOffset.UtcNow));
    }
}
