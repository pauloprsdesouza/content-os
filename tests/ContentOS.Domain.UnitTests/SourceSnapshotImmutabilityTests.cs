using ContentOS.Domain.Knowledge;
using Shouldly;

namespace ContentOS.Domain.UnitTests;

public sealed class SourceSnapshotImmutabilityTests
{
    [Fact]
    public void Snapshot_retains_content_hash_and_source()
    {
        var sourceId = Guid.CreateVersion7();
        var hash = ContentHash.Create(new string('a', 64));
        var snapshot = SourceSnapshot.Create(
            Guid.CreateVersion7(),
            sourceId,
            hash,
            "text/plain",
            12,
            DateTimeOffset.UtcNow);

        snapshot.SourceId.ShouldBe(sourceId);
        snapshot.ContentHash.ShouldBe(hash);
        snapshot.MediaType.ShouldBe("text/plain");
        snapshot.ByteLength.ShouldBe(12);
    }
}
