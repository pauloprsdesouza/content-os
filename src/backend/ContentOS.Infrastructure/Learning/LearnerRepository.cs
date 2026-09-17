using ContentOS.Application.Learning.Ports;
using ContentOS.Domain.Learning;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Learning;

public sealed class LearnerRepository(PlatformDbContext dbContext) : ILearnerRepository
{
    public Task<Learner?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<Learner>()
            .FirstOrDefaultAsync(
                learner => learner.Email == email.Trim().ToLowerInvariant(),
                cancellationToken);

    public void Add(Learner learner) =>
        dbContext.Set<Learner>().Add(learner);
}
