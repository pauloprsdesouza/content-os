using ContentOS.SharedKernel;
using Shouldly;

namespace ContentOS.Domain.UnitTests;

public sealed class IdempotencyKeyTests
{
    [Fact]
    public void Create_trims_and_preserves_the_key()
    {
        var key = IdempotencyKey.Create("  request-123  ");

        key.Value.ShouldBe("request-123");
        key.ToString().ShouldBe("request-123");
    }
}
