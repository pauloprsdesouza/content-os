using ContentOS.Domain.Content;

namespace ContentOS.Application.Content.Formats;

public sealed class NewsletterFormatStrategy : IContentFormatStrategy
{
    public ContentFormat Format => ContentFormat.Newsletter;

    public string DisplayName => "Newsletter";

    public string BuildMold() =>
        """
        Estrutura: assunto, abertura, três pontos, o que fazer agora.
        Tamanho: 400 a 700 palavras.
        Cite só trechos fornecidos, pelo identificador da obra ou do hash. Não invente fonte e não inclua URL.
        """;
}
