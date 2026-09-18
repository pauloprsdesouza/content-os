using ContentOS.Domain.Content;

namespace ContentOS.Application.Content.Ports;

public interface ITopicDiscoveryRepository
{
    void Add(TopicDiscovery discovery);

    void AddWorks(IEnumerable<DiscoveredWork> works);

    void AddProposals(IEnumerable<TopicProposal> proposals);

    Task<TopicDiscovery?> GetByIdAsync(Guid discoveryId, CancellationToken cancellationToken = default);

    Task<TopicDiscovery?> FindScheduledAsync(
        Guid seriesId,
        DateTimeOffset scheduledFor,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DiscoveredWork>> ListWorksAsync(
        Guid discoveryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TopicProposal>> ListProposalsAsync(
        Guid discoveryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TopicDiscovery>> ListForOwnerAsync(
        Guid ownerUserId,
        TopicDiscoveryStatus? status,
        CancellationToken cancellationToken = default);
}
