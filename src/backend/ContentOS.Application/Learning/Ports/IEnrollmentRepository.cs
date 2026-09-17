using ContentOS.Domain.Learning;

namespace ContentOS.Application.Learning.Ports;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(Guid enrollmentId, CancellationToken cancellationToken = default);

    void Add(Enrollment enrollment);
}
