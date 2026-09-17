using ContentOS.Application.Knowledge.Sources.GetSource;
using ContentOS.Contracts.Knowledge;

namespace ContentOS.Api.Knowledge.Sources;

public static class GetSourceEndpoint
{
    public static RouteGroupBuilder MapGetSource(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/{sourceId:guid}",
            async (
                Guid sourceId,
                GetSourceHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new GetSourceQuery(sourceId),
                    cancellationToken);

                if (!result.Found)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Source not found",
                        extensions: new Dictionary<string, object?>
                        {
                            ["code"] = "SOURCE_NOT_FOUND"
                        });
                }

                return Results.Ok(
                    new SourceResponse(
                        result.Id!.Value,
                        result.DisplayName!,
                        result.CanonicalUri!,
                        result.Kind!.Value.ToString(),
                        result.CreatedAt!.Value,
                        result.UpdatedAt!.Value));
            })
            .RequireAuthorization();

        return group;
    }
}
