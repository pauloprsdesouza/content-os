using ContentOS.Application.Learning.Ports;
using ContentOS.Domain.Learning;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Learning;

public sealed class EnrollmentRepository(PlatformDbContext dbContext) : IEnrollmentRepository
{
    public Task<Enrollment?> GetByIdAsync(
        Guid enrollmentId,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<Enrollment>()
            .FirstOrDefaultAsync(enrollment => enrollment.Id == enrollmentId, cancellationToken);

    public void Add(Enrollment enrollment) =>
        dbContext.Set<Enrollment>().Add(enrollment);
}
