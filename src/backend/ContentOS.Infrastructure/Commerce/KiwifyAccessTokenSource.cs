using System.Net.Http.Headers;
using System.Text.Json;
using ContentOS.Application.Commerce.Ports;
using ContentOS.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ContentOS.Infrastructure.Commerce;

public sealed class KiwifyAccessTokenSource(
    IHttpClientFactory httpClientFactory,
    IOptions<CommerceOptions> options)
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private string? _token;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    public async Task<string> GetAsync(CancellationToken cancellationToken)
    {
        if (IsFresh())
        {
            return _token!;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (IsFresh())
            {
                return _token!;
            }

            var commerce = options.Value;
            var client = httpClientFactory.CreateClient(HttpKiwifyOrderReconciler.HttpClientName);
            client.BaseAddress = new Uri(commerce.BaseUrl.TrimEnd('/') + "/");
            using var request = new HttpRequestMessage(HttpMethod.Post, "v1/oauth/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = commerce.ClientId ?? string.Empty,
                    ["client_secret"] = commerce.ClientSecret ?? commerce.ApiKey ?? string.Empty
                })
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var token = document.RootElement.GetProperty("access_token").GetString();
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new CommerceProviderNotConfiguredException("Kiwify did not return an access token.");
            }

            var expiresIn = document.RootElement.TryGetProperty("expires_in", out var expiresNode)
                && expiresNode.TryGetInt32(out var seconds)
                ? seconds
                : 3600;
            _token = token;
            _expiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(60, expiresIn - 120));
            return _token;
        }
        finally
        {
            _gate.Release();
        }
    }

    private bool IsFresh() =>
        !string.IsNullOrWhiteSpace(_token) && _expiresAt > DateTimeOffset.UtcNow.AddMinutes(1);
}
