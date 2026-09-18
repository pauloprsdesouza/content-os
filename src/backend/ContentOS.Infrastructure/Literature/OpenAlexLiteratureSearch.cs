using ContentOS.Application.Content.Literature;
using ContentOS.Infrastructure.Options;

namespace ContentOS.Infrastructure.Literature;

public sealed class OpenAlexLiteratureSearch(HttpClient httpClient, OpenAlexOptions options) : IScholarlyLiterature
{
    public const string HttpClientName = "openalex";

    public Task<ScholarlyLookupResult<ScholarlyArea>> ListFieldsAsync(CancellationToken cancellationToken = default) =>
        GetAreasAsync(WithAuth("fields?per-page=200&select=id,display_name"), cancellationToken);

    public Task<ScholarlyLookupResult<ScholarlyArea>> ListSubfieldsAsync(
        string fieldId,
        CancellationToken cancellationToken = default)
    {
        if (!OpenAlexIdentifiers.TryNormalizeArea(fieldId, out var shortId, out var kind) || kind != "fields")
        {
            return Task.FromResult(ScholarlyLookupResult<ScholarlyArea>.Fail("LITERATURE_AREA_INVALID"));
        }

        return GetAreasAsync(
            WithAuth($"subfields?filter={Uri.EscapeDataString($"field.id:{shortId}")}&per-page=200&select=id,display_name"),
            cancellationToken);
    }

    public async Task<ScholarlyLookupResult<ScholarlyWork>> SearchRecentWorksAsync(
        ScholarlyWorkQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!OpenAlexIdentifiers.TryNormalizeArea(query.AreaId, out var shortId, out var kind))
        {
            return ScholarlyLookupResult<ScholarlyWork>.Fail("LITERATURE_AREA_INVALID");
        }

        if (query.IsSubfield && kind != "subfields" || !query.IsSubfield && kind != "fields")
        {
            return ScholarlyLookupResult<ScholarlyWork>.Fail("LITERATURE_AREA_INVALID");
        }

        if (query.WindowDays is not (7 or 30 or 90))
        {
            return ScholarlyLookupResult<ScholarlyWork>.Fail("TOPIC_WINDOW_INVALID");
        }

        var from = DateTime.UtcNow.Date.AddDays(-query.WindowDays).ToString("yyyy-MM-dd");
        var filterField = query.IsSubfield ? "primary_topic.subfield.id" : "primary_topic.field.id";
        var perPage = Math.Clamp(options.MaxWorks, 1, 50);
        var path = WithAuth(
            "works?filter="
            + Uri.EscapeDataString($"{filterField}:{shortId},from_publication_date:{from}")
            + $"&sort=publication_date:desc&per-page={perPage}&select=id,display_name,publication_year,abstract_inverted_index,primary_topic");
        try
        {
            using var response = await httpClient.GetAsync(path, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return ScholarlyLookupResult<ScholarlyWork>.Fail("LITERATURE_UNAVAILABLE");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return ScholarlyLookupResult<ScholarlyWork>.Ok(OpenAlexPayloadReader.ReadWorks(json));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return ScholarlyLookupResult<ScholarlyWork>.Fail("LITERATURE_UNAVAILABLE");
        }
    }

    private async Task<ScholarlyLookupResult<ScholarlyArea>> GetAreasAsync(
        string path,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(path, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return ScholarlyLookupResult<ScholarlyArea>.Fail("LITERATURE_UNAVAILABLE");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return ScholarlyLookupResult<ScholarlyArea>.Ok(OpenAlexPayloadReader.ReadAreas(json));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return ScholarlyLookupResult<ScholarlyArea>.Fail("LITERATURE_UNAVAILABLE");
        }
    }

    private string WithAuth(string path)
    {
        var extras = new List<string>();
        if (!string.IsNullOrWhiteSpace(options.MailTo))
        {
            extras.Add("mailto=" + Uri.EscapeDataString(options.MailTo));
        }

        if (!string.IsNullOrWhiteSpace(options.ApiKey))
        {
            extras.Add("api_key=" + Uri.EscapeDataString(options.ApiKey));
        }

        return extras.Count == 0 ? path : path + "&" + string.Join('&', extras);
    }
}
