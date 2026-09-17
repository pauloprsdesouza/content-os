using ContentOS.Application.Learning.Ports;
using ContentOS.Domain.Learning;
using ContentOS.Infrastructure.Persistence;

namespace ContentOS.Infrastructure.Learning;

public sealed class OutcomeRepository(PlatformDbContext dbContext) : IOutcomeRepository
{
    public void Add(Outcome outcome) =>
        dbContext.Set<Outcome>().Add(outcome);
}
