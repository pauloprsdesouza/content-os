using ContentOS.Application.Learning.Ports;
using ContentOS.Domain.Learning;
using ContentOS.Infrastructure.Persistence;

namespace ContentOS.Infrastructure.Learning;

public sealed class EvaluationRepository(PlatformDbContext dbContext) : IEvaluationRepository
{
    public void Add(Evaluation evaluation) =>
        dbContext.Set<Evaluation>().Add(evaluation);
}
