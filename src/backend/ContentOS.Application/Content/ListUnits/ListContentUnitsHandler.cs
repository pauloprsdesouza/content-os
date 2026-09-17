using ContentOS.Application.Content.Ports;

namespace ContentOS.Application.Content.ListUnits;

public sealed class ListContentUnitsHandler(IContentUnitsQuery query)
{
    public async Task<ListContentUnitsResult> HandleAsync(
        ListContentUnitsQuery request,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var result = await query.GetPageAsync(page, pageSize, cancellationToken);
        return new ListContentUnitsResult(result);
    }
}
