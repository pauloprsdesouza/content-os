using ContentOS.Domain.Research;

namespace ContentOS.Application.Research.Ports;

public interface IResearchJobRepository
{
    Task<ResearchJob?> GetByIdAsync(
        Guid researchJobId,
        CancellationToken cancellationToken = default);

    void Add(ResearchJob job);

    Task DeleteGraphAsync(Guid researchJobId, CancellationToken cancellationToken = default);
}
