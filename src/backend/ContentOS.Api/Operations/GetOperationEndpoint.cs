using ContentOS.Application.Operations.Get;
using ContentOS.Contracts.Common;

namespace ContentOS.Api.Operations;

public static class GetOperationEndpoint
{
    public static RouteGroupBuilder MapGetOperation(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/{id:guid}",
            async (
                Guid id,
                GetOperationHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new GetOperationQuery(id),
                    cancellationToken);

                if (!result.IsSuccess || result.Operation is null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Operation not found",
                        extensions: new Dictionary<string, object?>
                        {
                            ["code"] = "OPERATION_NOT_FOUND"
                        });
                }

                var operation = result.Operation;
                return Results.Ok(
                    new OperationResponse(
                        operation.Id,
                        operation.Kind,
                        operation.SubjectType,
                        operation.SubjectId,
                        operation.Status.ToString(),
                        operation.ErrorMessage,
                        operation.CreatedAt,
                        operation.UpdatedAt));
            })
            .RequireAuthorization();

        return group;
    }
}
