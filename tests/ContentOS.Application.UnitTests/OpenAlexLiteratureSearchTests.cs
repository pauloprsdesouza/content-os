using ContentOS.Application.Content.Literature;
using ContentOS.Infrastructure.Literature;
using ContentOS.Infrastructure.Options;
using Shouldly;

namespace ContentOS.Application.UnitTests;

public sealed class OpenAlexLiteratureSearchTests
{
    [Fact]
    public async Task Recorded_response_reconstructs_abstract_and_short_work_id()
    {
        var json = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures", "openalex-works.json"));
        var handler = new RecordedJsonHandler(json);
        using var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.openalex.org/")
        };
        var search = new OpenAlexLiteratureSearch(
            http,
            new OpenAlexOptions
            {
                BaseUrl = "https://api.openalex.org",
                MaxWorks = 10,
                TimeoutSeconds = 5
            });

        var result = await search.SearchRecentWorksAsync(new ScholarlyWorkQuery("fields/17", false, 30));

        result.IsSuccess.ShouldBeTrue();
        result.Items.Count.ShouldBe(1);
        result.Items[0].WorkId.ShouldBe("W2741809807");
        result.Items[0].AbstractText.ShouldBe("Hello world");
        result.Items[0].TopicId.ShouldBe("T100");
        result.Items[0].TopicName.ShouldBe("Nutrition policy");
        handler.RequestUri!.Host.ShouldBe("api.openalex.org");
        handler.RequestUri.AbsolutePath.ShouldBe("/works");
        handler.RequestUri.Query.ShouldContain("from_publication_date");
    }
}
