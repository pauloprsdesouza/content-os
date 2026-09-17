using ContentOS.Contracts.Common;

namespace ContentOS.Api.Http;

public static class PageResponseFactory
{
    public static PageResponse<T> Create<T>(
        IReadOnlyList<T> items,
        int page,
        int pageSize,
        long totalItems)
    {
        var totalPages = pageSize <= 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PageResponse<T>(items, page, pageSize, totalItems, totalPages);
    }
}
