namespace ContentOS.Api.Http;

public static class ResultsETagExtensions
{
    public static IResult WithETag(this IResult result, string etag) =>
        new ETagResult(result, etag);
}
