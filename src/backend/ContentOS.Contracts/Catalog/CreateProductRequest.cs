namespace ContentOS.Contracts.Catalog;

public sealed record CreateProductRequest(string Name, string? Description, string? EditionName);
