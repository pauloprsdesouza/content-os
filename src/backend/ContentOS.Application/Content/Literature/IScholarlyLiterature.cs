namespace ContentOS.Application.Content.Literature;

public interface IScholarlyLiterature
{
    Task<ScholarlyLookupResult<ScholarlyArea>> ListFieldsAsync(CancellationToken cancellationToken = default);

    Task<ScholarlyLookupResult<ScholarlyArea>> ListSubfieldsAsync(
        string fieldId,
        CancellationToken cancellationToken = default);

    Task<ScholarlyLookupResult<ScholarlyWork>> SearchRecentWorksAsync(
        ScholarlyWorkQuery query,
        CancellationToken cancellationToken = default);
}
