using System.Net;
using System.Net.Sockets;
using System.Text;
using ContentOS.Application.Knowledge.Capture;
using ContentOS.Application.Knowledge.Ports;
using ContentOS.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ContentOS.Infrastructure.Knowledge;

public sealed class WebPageSourceCapture(
    IHttpClientFactory httpClientFactory,
    IOptions<IngestOptions> options) : ISourceCapture
{
    public const string HttpClientName = "source-capture";

    public SourceCaptureMode Mode => SourceCaptureMode.Web;

    public async Task<SourceCaptureOutcome> CaptureAsync(
        SourceCaptureRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Location is null || request.Location.Scheme is not ("http" or "https"))
        {
            return SourceCaptureOutcome.Fail("SOURCE_INVALID_URI");
        }

        if (IPAddress.TryParse(request.Location.IdnHost, out var literal)
            && SsrfAddressPolicy.IsBlocked(literal))
        {
            return SourceCaptureOutcome.Fail("CAPTURE_BLOCKED");
        }

        var limits = options.Value;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(limits.TimeoutSeconds, 1, 60)));

        try
        {
            return await DownloadAsync(request.Location, limits, timeout.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return SourceCaptureOutcome.Fail("CAPTURE_UNREACHABLE");
        }
        catch (HttpRequestException exception) when (exception.Message.Contains("CAPTURE_BLOCKED", StringComparison.Ordinal))
        {
            return SourceCaptureOutcome.Fail("CAPTURE_BLOCKED");
        }
        catch (HttpRequestException)
        {
            return SourceCaptureOutcome.Fail("CAPTURE_UNREACHABLE");
        }
        catch (SocketException)
        {
            return SourceCaptureOutcome.Fail("CAPTURE_UNREACHABLE");
        }
    }

    private async Task<SourceCaptureOutcome> DownloadAsync(
        Uri location,
        IngestOptions limits,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(HttpClientName);
        var current = location;
        var maxRedirects = Math.Clamp(limits.MaxRedirects, 0, 5);

        for (var hop = 0; hop <= maxRedirects; hop++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, current);
            using var response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if ((int)response.StatusCode is >= 300 and < 400)
            {
                var next = response.Headers.Location;
                if (next is null || hop == maxRedirects)
                {
                    return SourceCaptureOutcome.Fail("CAPTURE_REDIRECT");
                }

                if (!next.IsAbsoluteUri)
                {
                    next = new Uri(current, next);
                }

                if (next.Scheme is not ("http" or "https")
                    || (IPAddress.TryParse(next.IdnHost, out var redirected)
                        && SsrfAddressPolicy.IsBlocked(redirected)))
                {
                    return SourceCaptureOutcome.Fail("CAPTURE_BLOCKED");
                }

                current = next;
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                return SourceCaptureOutcome.Fail("CAPTURE_HTTP");
            }

            var maxBytes = Math.Clamp(limits.MaxBytes, 1024, 5_000_000);
            var payload = await ReadLimitedAsync(response.Content, maxBytes, cancellationToken);
            if (payload is null)
            {
                return SourceCaptureOutcome.Fail("CAPTURE_TOO_LARGE");
            }

            var mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
            var body = Encoding.UTF8.GetString(payload);
            if (mediaType.Contains("html", StringComparison.OrdinalIgnoreCase)
                || body.TrimStart().StartsWith('<'))
            {
                var text = HtmlTextExtractor.Extract(body);
                if (text.Length < Math.Clamp(limits.MinimumExtractedChars, 1, 500))
                {
                    return SourceCaptureOutcome.Fail("CAPTURE_NO_TEXT");
                }

                return SourceCaptureOutcome.Ok(
                    new MemoryStream(Encoding.UTF8.GetBytes(text)),
                    "text/plain");
            }

            if (mediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(body))
                {
                    return SourceCaptureOutcome.Fail("CAPTURE_EMPTY");
                }

                return SourceCaptureOutcome.Ok(new MemoryStream(payload), "text/plain");
            }

            return SourceCaptureOutcome.Fail("CAPTURE_UNSUPPORTED_MEDIA");
        }

        return SourceCaptureOutcome.Fail("CAPTURE_REDIRECT");
    }

    private static async Task<byte[]?> ReadLimitedAsync(
        HttpContent content,
        int maxBytes,
        CancellationToken cancellationToken)
    {
        await using var stream = await content.ReadAsStreamAsync(cancellationToken);
        var buffer = new byte[maxBytes + 1];
        var read = 0;
        while (read < buffer.Length)
        {
            var count = await stream.ReadAsync(buffer.AsMemory(read), cancellationToken);
            if (count == 0)
            {
                break;
            }

            read += count;
        }

        if (read > maxBytes)
        {
            return null;
        }

        return buffer.AsSpan(0, read).ToArray();
    }

    public static SocketsHttpHandler CreateHandler()
    {
        return new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            ConnectCallback = ConnectAsync
        };
    }

    private static async ValueTask<Stream> ConnectAsync(
        SocketsHttpConnectionContext context,
        CancellationToken cancellationToken)
    {
        var host = context.DnsEndPoint.Host;
        var addresses = IPAddress.TryParse(host, out var literal)
            ? new[] { literal }
            : await Dns.GetHostAddressesAsync(host, cancellationToken);
        var allowed = addresses.Where(address => !SsrfAddressPolicy.IsBlocked(address)).ToArray();
        if (allowed.Length == 0)
        {
            throw new HttpRequestException("CAPTURE_BLOCKED");
        }

        var socket = new Socket(allowed[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            await socket.ConnectAsync(allowed[0], context.DnsEndPoint.Port, cancellationToken);
            return new NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }
}
