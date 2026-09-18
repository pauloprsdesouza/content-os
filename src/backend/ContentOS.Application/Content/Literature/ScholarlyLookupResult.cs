namespace ContentOS.Application.Content.Literature;

public sealed record ScholarlyLookupResult<T>(
    bool IsSuccess,
    string? ErrorCode,
    IReadOnlyList<T> Items)
{
    public static ScholarlyLookupResult<T> Ok(IReadOnlyList<T> items) => new(true, null, items);

    public static ScholarlyLookupResult<T> Fail(string errorCode) => new(false, errorCode, []);
}
