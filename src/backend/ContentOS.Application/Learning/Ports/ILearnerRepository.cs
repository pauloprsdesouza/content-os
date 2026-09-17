using ContentOS.Domain.Learning;

namespace ContentOS.Application.Learning.Ports;

public interface ILearnerRepository
{
    Task<Learner?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    void Add(Learner learner);
}
