using ContentOS.Application.Persistence;

namespace ContentOS.Infrastructure.Persistence;

public sealed class EfChangeCommitter(PlatformDbContext dbContext) : IChangeCommitter
{
    public Task CommitAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
