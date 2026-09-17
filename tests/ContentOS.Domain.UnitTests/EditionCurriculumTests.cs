using ContentOS.Domain.Catalog;
using ContentOS.Domain.Publication;
using Shouldly;

namespace ContentOS.Domain.UnitTests;

public sealed class EditionCurriculumTests
{
    [Fact]
    public void ReplaceCurriculum_stores_explicit_ordered_versions()
    {
        var edition = Edition.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Edição 2026",
            DateTimeOffset.UtcNow);
        var first = Guid.CreateVersion7();
        var second = Guid.CreateVersion7();

        edition.ReplaceCurriculum([first, second], edition.Version, DateTimeOffset.UtcNow);

        edition.Curriculum.Count.ShouldBe(2);
        edition.Curriculum[0].Position.ShouldBe(0);
        edition.Curriculum[0].ContentVersionId.ShouldBe(first);
        edition.Curriculum[1].Position.ShouldBe(1);
        edition.Curriculum[1].ContentVersionId.ShouldBe(second);
        edition.Version.ShouldBe(2);
    }

    [Fact]
    public void ReplaceCurriculum_rejects_duplicates()
    {
        var edition = Edition.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Edição",
            DateTimeOffset.UtcNow);
        var versionId = Guid.CreateVersion7();

        var act = () => edition.ReplaceCurriculum(
            [versionId, versionId],
            edition.Version,
            DateTimeOffset.UtcNow);

        Should.Throw<InvalidOperationException>(act);
    }

    [Fact]
    public void ReplaceCurriculum_rejects_stale_version()
    {
        var edition = Edition.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Edição",
            DateTimeOffset.UtcNow);

        var act = () => edition.ReplaceCurriculum(
            [Guid.CreateVersion7()],
            expectedVersion: 99,
            DateTimeOffset.UtcNow);

        Should.Throw<InvalidOperationException>(act);
    }
}
