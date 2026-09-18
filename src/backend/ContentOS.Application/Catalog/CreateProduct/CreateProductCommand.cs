namespace ContentOS.Application.Catalog.CreateProduct;

public sealed record CreateProductCommand(string Name, string? Description, string? EditionName);
