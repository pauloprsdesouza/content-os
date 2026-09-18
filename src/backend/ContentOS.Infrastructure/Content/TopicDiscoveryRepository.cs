using ContentOS.Application.Content.Ports;
using ContentOS.Domain.Content;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Content;

public sealed class TopicDiscoveryRepository(PlatformDbContext dbContext) : ITopicDiscoveryRepository
{
    public void Add(TopicDiscovery discovery) => dbContext.Set<TopicDiscovery>().Add(discovery);

    public void AddWorks(IEnumerable<DiscoveredWork> works) => dbContext.Set<DiscoveredWork>().AddRange(works);

    public void AddProposals(IEnumerable<TopicProposal> proposals) =>
        dbContext.Set<TopicProposal>().AddRange(proposals);

    public Task<TopicDiscovery?> GetByIdAsync(Guid discoveryId, CancellationToken cancellationToken = default) =>
        dbContext.Set<TopicDiscovery>().FirstOrDefaultAsync(item => item.Id == discoveryId, cancellationToken);

    public Task<TopicDiscovery?> FindScheduledAsync(
        Guid seriesId,
        DateTimeOffset scheduledFor,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<TopicDiscovery>().FirstOrDefaultAsync(
            item => item.SeriesId == seriesId && item.ScheduledFor == scheduledFor,
            cancellationToken);

    public async Task<IReadOnlyList<DiscoveredWork>> ListWorksAsync(
        Guid discoveryId,
        CancellationToken cancellationToken = default) =>
        await dbContext.Set<DiscoveredWork>()
            .Where(work => work.DiscoveryId == discoveryId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TopicProposal>> ListProposalsAsync(
        Guid discoveryId,
        CancellationToken cancellationToken = default) =>
        await dbContext.Set<TopicProposal>()
            .Where(proposal => proposal.DiscoveryId == discoveryId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TopicDiscovery>> ListForOwnerAsync(
        Guid ownerUserId,
        TopicDiscoveryStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<TopicDiscovery>().AsNoTracking().Where(item => item.OwnerUserId == ownerUserId);
        if (status is TopicDiscoveryStatus known)
        {
            query = query.Where(item => item.Status == known);
        }

        return await query.OrderByDescending(item => item.UpdatedAt).Take(50).ToListAsync(cancellationToken);
    }
}
