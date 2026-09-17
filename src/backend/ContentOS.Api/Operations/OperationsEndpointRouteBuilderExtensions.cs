namespace ContentOS.Api.Operations;

public static class OperationsEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapOperationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var operations = endpoints.MapGroup("/api/v1/operations");
        operations.MapGetOperation();

        return endpoints;
    }
}
