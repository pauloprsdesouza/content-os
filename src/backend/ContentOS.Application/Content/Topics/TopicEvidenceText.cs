using System.Text;
using System.Text.RegularExpressions;
using ContentOS.Domain.Content;

namespace ContentOS.Application.Content.Topics;

public static partial class TopicEvidenceText
{
    public static string Build(string label, string? rationale, IReadOnlyList<DiscoveredWork> works)
    {
        var builder = new StringBuilder();
        builder.Append("Tema: ").AppendLine(label);
        if (!string.IsNullOrWhiteSpace(rationale))
        {
            builder.Append("Por que: ").AppendLine(rationale);
        }

        foreach (var work in works)
        {
            builder.AppendLine();
            builder.Append("Obra: ").AppendLine(work.WorkId);
            builder.Append("Título: ").AppendLine(work.Title);
            if (work.PublicationYear is int year)
            {
                builder.Append("Ano: ").AppendLine(year.ToString());
            }

            if (!string.IsNullOrWhiteSpace(work.TopicName))
            {
                builder.Append("Tópico: ").AppendLine(work.TopicName);
            }

            builder.AppendLine("Resumo:");
            builder.AppendLine(work.AbstractText);
        }

        return UrlPattern().Replace(builder.ToString(), string.Empty).Trim();
    }

    [GeneratedRegex(@"https?://\S+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex UrlPattern();
}
