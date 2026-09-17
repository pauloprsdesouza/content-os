using ContentOS.Api.Http;
using ContentOS.Application.Commerce.ListPurchases;
using ContentOS.Contracts.Commerce;

namespace ContentOS.Api.Commerce;

public static class ListPurchasesEndpoint
{
    public static RouteGroupBuilder MapListPurchases(this RouteGroupBuilder group)
    {
        group.MapGet(
                "/",
                async (
                    [AsParameters] PaginationQuery pagination,
                    ListPurchasesHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        new ListPurchasesQuery(pagination.Page, pagination.PageSize),
                        cancellationToken);

                    var items = result.Page.Items
                        .Select(item => new PurchaseListItemResponse(
                            item.Id,
                            item.Provider,
                            item.ExternalId,
                            item.Status,
                            item.BuyerEmail,
                            item.ProductId,
                            item.EditionId,
                            item.LearnerId,
                            item.WebhookInboxEntryId,
                            item.ReconciliationRunId,
                            item.ConfirmedAt,
                            item.CreatedAt,
                            item.UpdatedAt))
                        .ToArray();

                    return Results.Ok(
                        PageResponseFactory.Create(
                            items,
                            result.Page.Page,
                            result.Page.PageSize,
                            result.Page.TotalItems));
                })
            .RequireAuthorization();

        return group;
    }
}
