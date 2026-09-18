using ContentOS.Infrastructure.Commerce;
using Shouldly;

namespace ContentOS.Application.UnitTests;

public sealed class KiwifyProductMapTests
{
    [Fact]
    public void Mapped_product_resolves_edition()
    {
        var productId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var editionId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var map = KiwifyProductMap.Parse($"ext-1={productId}|{editionId}");

        map.TryGet("ext-1", out var product, out var edition).ShouldBeTrue();
        product.ShouldBe(productId);
        edition.ShouldBe(editionId);
    }

    [Fact]
    public void Unknown_product_is_not_mapped()
    {
        var map = KiwifyProductMap.Parse(null);
        map.TryGet("missing", out _, out _).ShouldBeFalse();
    }
}
