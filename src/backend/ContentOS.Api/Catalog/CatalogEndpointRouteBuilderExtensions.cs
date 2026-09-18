namespace ContentOS.Api.Catalog;

public static class CatalogEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var products = endpoints.MapGroup("/api/v1/products");
        products.MapListProducts();
        products.MapCreateProduct();
        products.MapDeleteProduct();
        products.MapGetEdition();

        var editions = endpoints.MapGroup("/api/v1/editions");
        editions.MapReplaceCurriculum();

        return endpoints;
    }
}
