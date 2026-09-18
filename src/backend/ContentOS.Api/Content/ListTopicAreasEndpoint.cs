using ContentOS.Application.Content.Literature;
using ContentOS.Contracts.Content;

namespace ContentOS.Api.Content;

public static class ListTopicAreasEndpoint
{
    public static RouteGroupBuilder MapListTopicAreas(this RouteGroupBuilder group)
    {
        group.MapGet(
                "/",
                async (
                    string? parentId,
                    IScholarlyLiterature literature,
                    CancellationToken cancellationToken) =>
                {
                    var result = string.IsNullOrWhiteSpace(parentId)
                        ? await literature.ListFieldsAsync(cancellationToken)
                        : await literature.ListSubfieldsAsync(parentId, cancellationToken);
                    if (!result.IsSuccess)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status502BadGateway,
                            title: "Literature lookup failed",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            });
                    }

                    return Results.Ok(result.Items.Select(area => new TopicAreaResponse(area.Id, area.Name)).ToArray());
                })
            .RequireAuthorization();

        return group;
    }
}
