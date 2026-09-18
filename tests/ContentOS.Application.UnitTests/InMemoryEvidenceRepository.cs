using ContentOS.Application.Knowledge.Ports;
using ContentOS.Domain.Knowledge;

namespace ContentOS.Application.UnitTests;

public sealed class InMemoryEvidenceRepository : IEvidenceRepository
{
    public List<Evidence> Items { get; } = [];

    public Task<IReadOnlyList<Evidence>> GetByIdsAsync(
        IReadOnlyCollection<Guid> evidenceIds,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Evidence>>(Items.Where(item => evidenceIds.Contains(item.Id)).ToList());

    public void Add(Evidence evidence) => Items.Add(evidence);
}
