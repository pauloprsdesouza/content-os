using ContentOS.Application.Operations.Progress;
using ContentOS.Infrastructure.Operations;
using Shouldly;

namespace ContentOS.Application.UnitTests;

public sealed class OperationProgressBrokerTests
{
    [Fact]
    public async Task Publish_reaches_the_subscriber_for_that_operation()
    {
        var broker = new OperationProgressBroker();
        var operationId = Guid.NewGuid();
        await using var subscription = broker.Subscribe(operationId);

        broker.Publish(new OperationProgressNotice(operationId, "Succeeded", null, DateTimeOffset.UtcNow));
        broker.Publish(new OperationProgressNotice(Guid.NewGuid(), "Failed", "other", DateTimeOffset.UtcNow));

        subscription.TryRead(out var notice).ShouldBeTrue();
        notice.OperationId.ShouldBe(operationId);
        notice.Status.ShouldBe("Succeeded");
        subscription.TryRead(out _).ShouldBeFalse();
    }
}
