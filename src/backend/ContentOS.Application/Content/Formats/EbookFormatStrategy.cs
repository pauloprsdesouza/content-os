using ContentOS.Domain.Content;

namespace ContentOS.Application.Content.Formats;

public sealed class EbookFormatStrategy : IContentFormatStrategy
{
    public ContentFormat Format => ContentFormat.Ebook;

    public string DisplayName => "Ebook";

    public string BuildMold() =>
        """
        Estrutura: sumário e três capítulos curtos, cada um com uma evidência.
        Tamanho: 900 a 1400 palavras.
        Cite só trechos fornecidos, pelo identificador da obra ou do hash. Não invente fonte e não inclua URL.
        """;
}
