using ContentOS.Domain.Commerce;

namespace ContentOS.Application.Commerce.Ports;

public interface IWebhookInboxRepository
{
    Task<WebhookInboxEntry?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<WebhookInboxEntry?> GetByProviderExternalIdAsync(
        string provider,
        string externalId,
        CancellationToken cancellationToken = default);

    void Add(WebhookInboxEntry entry);
}
