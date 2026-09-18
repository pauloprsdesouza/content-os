using System.Net;
using System.Text;

namespace ContentOS.Application.UnitTests;

public sealed class RecordedJsonHandler : HttpMessageHandler
{
    private readonly string _json;

    public RecordedJsonHandler(string json) => _json = json;

    public Uri? RequestUri { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        RequestUri = request.RequestUri;
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(_json, Encoding.UTF8, "application/json")
        });
    }
}
