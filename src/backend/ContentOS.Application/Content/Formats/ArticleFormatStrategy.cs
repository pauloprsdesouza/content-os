using ContentOS.Domain.Content;

namespace ContentOS.Application.Content.Formats;

public sealed class ArticleFormatStrategy : IContentFormatStrategy
{
    public ContentFormat Format => ContentFormat.Article;

    public string DisplayName => "Artigo";

    public string BuildMold() =>
        """
        Estrutura: tese, desenvolvimento, limitações, referências.
        Tamanho: 800 a 1200 palavras.
        Cite só trechos fornecidos, pelo identificador da obra ou do hash. Não invente fonte e não inclua URL.
        """;
}
