using ContentOS.Application.Content.Topics;
using Shouldly;

namespace ContentOS.Application.UnitTests;

public sealed class TopicLabelGateTests
{
    [Fact]
    public void Topic_without_work_id_is_rejected()
    {
        var accepted = TopicLabelGate.Accept(
            new HashSet<string>(["W1"], StringComparer.Ordinal),
            [
                new ProposedTopicLabel("Invented", "no citation", []),
                new ProposedTopicLabel("Unknown", "not retrieved", ["W999"]),
                new ProposedTopicLabel("Grounded", "from the search", ["W1"])
            ]);

        accepted.Count.ShouldBe(1);
        accepted[0].Label.ShouldBe("Grounded");
        accepted[0].WorkIds.ShouldBe(["W1"]);
    }
}
