using ContentOS.Application.Learning.Ports;
using ContentOS.Domain.Learning;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Learning;

public sealed class CapstoneRepository(PlatformDbContext dbContext) : ICapstoneRepository
{
    public Task<Capstone?> GetByIdAsync(
        Guid capstoneId,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<Capstone>()
            .FirstOrDefaultAsync(capstone => capstone.Id == capstoneId, cancellationToken);

    public Task<Capstone?> GetByEnrollmentIdAsync(
        Guid enrollmentId,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<Capstone>()
            .FirstOrDefaultAsync(
                capstone => capstone.EnrollmentId == enrollmentId,
                cancellationToken);

    public void Add(Capstone capstone) =>
        dbContext.Set<Capstone>().Add(capstone);
}
