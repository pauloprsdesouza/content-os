using ContentOS.Domain.Learning;

namespace ContentOS.Application.Learning.Ports;

public interface IEvaluationRepository
{
    void Add(Evaluation evaluation);
}
