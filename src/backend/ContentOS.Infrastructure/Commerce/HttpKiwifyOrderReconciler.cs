using System.Net.Http.Headers;
using System.Text.Json;
using ContentOS.Application.Commerce.Ports;
using ContentOS.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContentOS.Infrastructure.Commerce;

public sealed class HttpKiwifyOrderReconciler(
    IHttpClientFactory httpClientFactory,
    IOptions<CommerceOptions> options,
    KiwifyAccessTokenSource tokens,
    ILogger<HttpKiwifyOrderReconciler> logger) : ICommerceOrderReconciler
{
    public const string HttpClientName = "kiwify";

    public async Task<CommerceOrderSnapshot> GetAuthoritativeOrderAsync(
        string provider,
        string externalId,
        string? webhookPayloadJson,
        CancellationToken cancellationToken = default)
    {
        _ = provider;
        var commerce = options.Value;
        var hasOAuth = !string.IsNullOrWhiteSpace(commerce.ClientId)
            && !string.IsNullOrWhiteSpace(commerce.ClientSecret ?? commerce.ApiKey);
        if (!hasOAuth && string.IsNullOrWhiteSpace(commerce.ApiKey))
        {
            throw new CommerceProviderNotConfiguredException(
                "Commerce:ClientId and Commerce:ClientSecret are required when Commerce:UseStubProvider is false.");
        }

        var client = httpClientFactory.CreateClient(HttpClientName);
        client.BaseAddress = new Uri(commerce.BaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(15);
        var bearer = hasOAuth
            ? await tokens.GetAsync(cancellationToken)
            : commerce.ApiKey!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
        if (!string.IsNullOrWhiteSpace(commerce.AccountId))
        {
            client.DefaultRequestHeaders.Remove("x-kiwify-account-id");
            client.DefaultRequestHeaders.TryAddWithoutValidation("x-kiwify-account-id", commerce.AccountId);
        }

        using var response = await client.GetAsync($"v1/sales/{Uri.EscapeDataString(externalId)}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Ignored(externalId, webhookPayloadJson);
        }

        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;
        var status = Read(root, "status") ?? Read(root, "order_status") ?? "unknown";
        var decision = MapDecision(status);
        var email = Read(root, "customer", "email") ?? "unknown@invalid.local";
        var name = Read(root, "customer", "name");
        var kiwifyProductId = Read(root, "product", "id") ?? Read(root, "product_id");
        var map = KiwifyProductMap.Parse(commerce.ProductMappings);
        Guid? productId = null;
        Guid? editionId = null;
        if (map.TryGet(kiwifyProductId, out var mappedProduct, out var mappedEdition))
        {
            productId = mappedProduct;
            editionId = mappedEdition;
        }
        else if (decision == CommerceReconcileDecision.Confirm)
        {
            logger.LogWarning(
                "Kiwify product {ProductId} is not mapped; order {ExternalId} stays pending.",
                kiwifyProductId ?? "unknown",
                externalId);
            decision = CommerceReconcileDecision.Ignore;
        }

        return new CommerceOrderSnapshot(externalId, decision, email, name, productId, editionId);
    }

    private static CommerceOrderSnapshot Ignored(string externalId, string? webhookPayloadJson)
    {
        _ = webhookPayloadJson;
        return new CommerceOrderSnapshot(
            externalId,
            CommerceReconcileDecision.Ignore,
            "unknown@invalid.local",
            null,
            null,
            null);
    }

    private static CommerceReconcileDecision MapDecision(string status) =>
        status.Trim().ToLowerInvariant() switch
        {
            "paid" or "approved" or "complete" or "completed" => CommerceReconcileDecision.Confirm,
            "refunded" or "refund" => CommerceReconcileDecision.Refund,
            "chargeback" => CommerceReconcileDecision.Chargeback,
            _ => CommerceReconcileDecision.Ignore
        };

    private static string? Read(JsonElement root, params string[] path)
    {
        var current = root;
        foreach (var segment in path)
        {
            if (current.ValueKind != JsonValueKind.Object
                || !current.TryGetProperty(segment, out current))
            {
                return null;
            }
        }

        return current.ValueKind == JsonValueKind.String ? current.GetString() : current.ToString();
    }
}
