namespace ContentOS.Application.Content.ListUnits;

public sealed record ListContentUnitsQuery(int Page, int PageSize, Guid? ProductId);
