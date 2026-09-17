using ContentOS.Domain.Learning;

namespace ContentOS.Application.Learning.Ports;

public interface ICapstoneRepository
{
    Task<Capstone?> GetByIdAsync(Guid capstoneId, CancellationToken cancellationToken = default);

    Task<Capstone?> GetByEnrollmentIdAsync(
        Guid enrollmentId,
        CancellationToken cancellationToken = default);

    void Add(Capstone capstone);
}
