using System.Text;
using ContentOS.Application.Commerce.ReceiveKiwifyWebhook;
using ContentOS.Contracts.Commerce;
using ContentOS.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContentOS.Api.Commerce;

public static class ReceiveKiwifyWebhookEndpoint
{
    public const string SignatureHeaderName = "X-Kiwify-Signature";

    public static RouteGroupBuilder MapReceiveKiwifyWebhook(this RouteGroupBuilder group)
    {
        group.MapPost(
            "/kiwify",
            async (
                HttpRequest request,
                ReceiveKiwifyWebhookHandler handler,
                IOptions<CommerceOptions> commerceOptions,
                IHostEnvironment environment,
                ILoggerFactory loggerFactory,
                CancellationToken cancellationToken) =>
            {
                string rawBody;
                using (var reader = new StreamReader(request.Body, Encoding.UTF8))
                {
                    rawBody = await reader.ReadToEndAsync(cancellationToken);
                }

                var secret = commerceOptions.Value.WebhookSecret;
                if (string.IsNullOrWhiteSpace(secret))
                {
                    loggerFactory.CreateLogger("ContentOS.Commerce.Webhook")
                        .LogWarning(
                            "Commerce:WebhookSecret is empty; accepting Kiwify webhook without signature verification ({Environment}).",
                            environment.EnvironmentName);
                }

                request.Headers.TryGetValue(SignatureHeaderName, out var signatureValues);
                var result = await handler.HandleAsync(
                    new ReceiveKiwifyWebhookCommand(
                        rawBody,
                        signatureValues.ToString(),
                        RequireSignature: false),
                    secret,
                    cancellationToken);

                if (!result.IsSuccess)
                {
                    return result.ErrorCode switch
                    {
                        "COMMERCE_WEBHOOK_INVALID_SIGNATURE" => Results.Problem(
                            statusCode: StatusCodes.Status401Unauthorized,
                            title: "Invalid webhook signature",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            }),
                        _ => Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Invalid webhook payload",
                            extensions: new Dictionary<string, object?>
                            {
                                ["code"] = result.ErrorCode
                            })
                    };
                }

                return Results.Accepted(
                    $"/api/v1/purchases/{result.PurchaseId}",
                    new KiwifyWebhookAcceptedResponse(
                        result.PurchaseId!.Value,
                        result.WebhookInboxEntryId!.Value,
                        result.AlreadyExisted,
                        StatusHint: "SignalReceived"));
            });

        return group;
    }
}
