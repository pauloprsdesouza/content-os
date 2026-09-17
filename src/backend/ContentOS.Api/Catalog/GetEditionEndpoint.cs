using ContentOS.Api.Http;
using ContentOS.Application.Catalog.GetEdition;
using ContentOS.Contracts.Catalog;

namespace ContentOS.Api.Catalog;

public static class GetEditionEndpoint
{
    public static RouteGroupBuilder MapGetEdition(this RouteGroupBuilder group)
    {
        group.MapGet(
                "/{productId:guid}/editions/{editionId:guid}",
                async (
                    Guid productId,
                    Guid editionId,
                    GetEditionHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        new GetEditionQuery(productId, editionId),
                        cancellationToken);

                    if (!result.IsSuccess || result.Edition is null)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status404NotFound,
                            title: "Edition not found",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            });
                    }

                    var edition = result.Edition;
                    var response = new EditionDetailResponse(
                        edition.Id,
                        edition.ProductId,
                        edition.ProductName,
                        edition.Name,
                        edition.Version,
                        edition.Curriculum
                            .Select(item => new CurriculumItemResponse(
                                item.Position,
                                item.ContentVersionId))
                            .ToArray(),
                        edition.CreatedAt,
                        edition.UpdatedAt);

                    return Results.Ok(response)
                        .WithETag(ETagFormatter.Format(edition.Version));
                })
            .RequireAuthorization();

        return group;
    }
}
