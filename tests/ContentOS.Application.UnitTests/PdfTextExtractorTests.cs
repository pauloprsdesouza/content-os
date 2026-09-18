using ContentOS.Infrastructure.Knowledge;
using Shouldly;

namespace ContentOS.Application.UnitTests;

public sealed class PdfTextExtractorTests
{
    [Fact]
    public void Extracts_text_from_a_readable_pdf()
    {
        using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", "readable.pdf"));

        var text = PdfTextExtractor.Extract(stream);

        text.ShouldNotBeNull();
        text.ShouldContain("Wolverine oferece mensageria duravel.");
    }

    [Fact]
    public void Returns_null_when_the_pdf_has_no_text_layer()
    {
        using var stream = new MemoryStream("%PDF-1.1\n1 0 obj<<>>endobj\ntrailer<<>>\n%%EOF"u8.ToArray());

        PdfTextExtractor.Extract(stream).ShouldBeNull();
    }
}
