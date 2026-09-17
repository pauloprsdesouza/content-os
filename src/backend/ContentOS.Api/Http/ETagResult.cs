namespace ContentOS.Api.Http;

public sealed class ETagResult(IResult inner, string etag) : IResult
{
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.Headers.ETag = etag;
        await inner.ExecuteAsync(httpContext);
    }
}
