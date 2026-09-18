using System.Text;
using UglyToad.PdfPig;

namespace ContentOS.Infrastructure.Knowledge;

public static class PdfTextExtractor
{
    public static string? Extract(Stream content)
    {
        ArgumentNullException.ThrowIfNull(content);

        try
        {
            using var document = PdfDocument.Open(content);
            var builder = new StringBuilder();
            foreach (var page in document.GetPages())
            {
                if (string.IsNullOrWhiteSpace(page.Text))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append('\n');
                }

                builder.Append(page.Text.Trim());
            }

            var text = builder.ToString().Trim();
            return text.Length == 0 ? null : text;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return null;
        }
    }
}
