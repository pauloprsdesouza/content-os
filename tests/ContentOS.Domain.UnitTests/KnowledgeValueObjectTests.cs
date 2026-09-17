using ContentOS.Domain.Knowledge;
using Shouldly;

namespace ContentOS.Domain.UnitTests;

public sealed class KnowledgeValueObjectTests
{
    [Fact]
    public void SourceUri_normalizes_https_host()
    {
        var uri = SourceUri.Create("https://Example.COM/Path?q=1#frag");

        uri.Value.Host.ShouldBe("example.com");
        uri.Value.Fragment.ShouldBeEmpty();
    }

    [Fact]
    public void ConfidenceScore_rejects_out_of_range()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => ConfidenceScore.Create(1.1m));
    }

    [Fact]
    public void ContentHash_requires_sha256_hex()
    {
        Should.Throw<ArgumentException>(() => ContentHash.Create("not-a-hash"));
    }
}
