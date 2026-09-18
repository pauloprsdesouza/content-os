using ContentOS.Domain.Content;

namespace ContentOS.Application.Content.Formats;

public sealed class PostFormatStrategy : IContentFormatStrategy
{
    public ContentFormat Format => ContentFormat.Post;

    public string DisplayName => "Post";

    public string BuildMold() =>
        """
        Estrutura: gancho, uma tese, um exemplo, fechamento.
        Tamanho: 150 a 250 palavras.
        Cite só trechos fornecidos, pelo identificador da obra ou do hash. Não invente fonte e não inclua URL.
        """;
}
