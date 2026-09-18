using ContentOS.Infrastructure.Knowledge;
using Shouldly;

namespace ContentOS.Application.UnitTests;

public sealed class HtmlTextExtractorTests
{
    [Fact]
    public void Strips_tags_and_scripts()
    {
        var text = HtmlTextExtractor.Extract(
            "<html><script>alert(1)</script><p>Wolverine oferece mensageria durável.</p></html>");

        text.ShouldContain("Wolverine oferece mensageria durável.");
        text.ShouldNotContain("alert");
        text.ShouldNotContain("<p>");
    }
}
