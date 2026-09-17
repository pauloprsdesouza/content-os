using ContentOS.Application.Research.Ports;

namespace ContentOS.Application.Research.List;

public sealed class ListResearchJobsHandler(IResearchJobsQuery query)
{
    public async Task<ListResearchJobsResult> HandleAsync(
        ListResearchJobsQuery request,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var result = await query.GetPageAsync(page, pageSize, cancellationToken);
        return new ListResearchJobsResult(result);
    }
}
