using ContentOS.Application.Commerce.Ports;
using ContentOS.Domain.Commerce;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Commerce;

public sealed class WebhookInboxRepository(PlatformDbContext dbContext) : IWebhookInboxRepository
{
    public Task<WebhookInboxEntry?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<WebhookInboxEntry>()
            .FirstOrDefaultAsync(entry => entry.Id == id, cancellationToken);

    public Task<WebhookInboxEntry?> GetByProviderExternalIdAsync(
        string provider,
        string externalId,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<WebhookInboxEntry>()
            .FirstOrDefaultAsync(
                entry => entry.Provider == provider && entry.ExternalId == externalId,
                cancellationToken);

    public void Add(WebhookInboxEntry entry) =>
        dbContext.Set<WebhookInboxEntry>().Add(entry);
}
