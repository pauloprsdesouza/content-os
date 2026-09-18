using ContentOS.Application.Persistence;

namespace ContentOS.Api.Http;

public static class DiscardHttp
{
    public static IResult ToResult(DiscardOutcome outcome)
    {
        if (outcome.Deleted)
        {
            return Results.NoContent();
        }

        var code = outcome.ErrorCode ?? "DISCARD_REJECTED";
        var (status, title) = code switch
        {
            "PRODUCT_IN_USE" => (StatusCodes.Status409Conflict, "Este produto já tem venda, matrícula ou publicação."),
            "SOURCE_IN_USE" => (StatusCodes.Status409Conflict, "Esta fonte já sustenta uma afirmação."),
            "RESEARCH_IN_REVIEW" => (StatusCodes.Status409Conflict, "Esta pesquisa já gerou afirmações. Revise-as em vez de apagar."),
            "CONTENT_IN_CURRICULUM" => (StatusCodes.Status409Conflict, "Este conteúdo está no currículo de um produto. Tire-o de lá antes de apagar."),
            _ when code.EndsWith("NOT_FOUND", StringComparison.Ordinal) => (StatusCodes.Status404NotFound, "Não encontrado."),
            _ => (StatusCodes.Status409Conflict, "Não foi possível apagar.")
        };

        return Results.Problem(
            statusCode: status,
            title: title,
            detail: title,
            extensions: new Dictionary<string, object?> { ["code"] = code });
    }
}
