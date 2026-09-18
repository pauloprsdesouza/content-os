using ContentOS.Domain.Content;

namespace ContentOS.Application.Content.Formats;

public sealed class LessonFormatStrategy : IContentFormatStrategy
{
    public ContentFormat Format => ContentFormat.Lesson;

    public string DisplayName => "Aula";

    public string BuildMold() =>
        """
        Estrutura: objetivo, pré-requisitos, três blocos, exercício, fontes.
        Tamanho: 600 a 900 palavras.
        Cite só trechos fornecidos, pelo identificador da obra ou do hash. Não invente fonte e não inclua URL.
        """;
}
