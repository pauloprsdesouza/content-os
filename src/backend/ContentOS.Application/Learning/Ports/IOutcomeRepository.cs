using ContentOS.Domain.Learning;

namespace ContentOS.Application.Learning.Ports;

public interface IOutcomeRepository
{
    void Add(Outcome outcome);
}
