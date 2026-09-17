using ContentOS.Domain.Publication;
using Shouldly;

namespace ContentOS.Domain.UnitTests;

public sealed class PublicationPackageTransitionTests
{
    [Fact]
    public void Full_path_to_published_confirmed()
    {
        var package = PublicationPackage.CreateRequested(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateTimeOffset.UtcNow);
        var now = DateTimeOffset.UtcNow;
        var manifest = CreateManifest();

        package.StartBuilding(now);
        package.MarkValidating(manifest, now);
        package.MarkReadyForExport(now);
        package.MarkExported("a".PadLeft(64, 'a'), 128, now);
        package.ConfirmPublication(Guid.CreateVersion7(), package.Version, now);

        package.Status.ShouldBe(PublicationPackageStatus.PublishedConfirmed);
        package.ManifestJson.ShouldNotBeNullOrWhiteSpace();
        package.ExportBlobSha256.ShouldNotBeNull();
        package.ConfirmedByUserId.ShouldNotBeNull();
    }

    [Fact]
    public void Export_does_not_imply_published_confirmed()
    {
        var package = BuildReadyPackage();
        var now = DateTimeOffset.UtcNow;

        package.MarkExported("b".PadLeft(64, 'b'), 10, now);

        package.Status.ShouldBe(PublicationPackageStatus.Exported);
        package.ConfirmedAt.ShouldBeNull();
    }

    [Fact]
    public void Confirm_from_ready_for_export_fails()
    {
        var package = BuildReadyPackage();

        var act = () => package.ConfirmPublication(
            Guid.CreateVersion7(),
            package.Version,
            DateTimeOffset.UtcNow);

        Should.Throw<InvalidOperationException>(act);
    }

    [Fact]
    public void Manifest_json_is_deterministic()
    {
        var contentVersionId = Guid.Parse("01999999-9999-7999-8999-999999999999");
        var first = PublicationPackageManifest.Create(
            PublicationRendererVersion.MvpV1,
            [],
            [new ManifestContentEntry(contentVersionId, "c".PadLeft(64, 'c'))]);
        var second = PublicationPackageManifest.Create(
            PublicationRendererVersion.MvpV1,
            [],
            [new ManifestContentEntry(contentVersionId, "c".PadLeft(64, 'c'))]);

        first.ToCanonicalJson().ShouldBe(second.ToCanonicalJson());
        first.ToCanonicalJson().ShouldContain("\"rendererVersion\":\"contentos.renderer.mvp.1\"");
        first.ToCanonicalJson().ShouldContain(contentVersionId.ToString("D"));
    }

    private static PublicationPackage BuildReadyPackage()
    {
        var package = PublicationPackage.CreateRequested(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateTimeOffset.UtcNow);
        var now = DateTimeOffset.UtcNow;
        package.StartBuilding(now);
        package.MarkValidating(CreateManifest(), now);
        package.MarkReadyForExport(now);
        return package;
    }

    private static PublicationPackageManifest CreateManifest() =>
        PublicationPackageManifest.Create(
            PublicationRendererVersion.MvpV1,
            [],
            [
                new ManifestContentEntry(
                    Guid.CreateVersion7(),
                    PublicationPackageManifest.HashContentBody("# Lesson"))
            ]);
}
