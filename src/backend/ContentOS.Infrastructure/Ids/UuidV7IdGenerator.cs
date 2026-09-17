using ContentOS.SharedKernel;

namespace ContentOS.Infrastructure.Ids;

public sealed class UuidV7IdGenerator(TimeProvider timeProvider) : IIdGenerator
{
    public Guid NewId() => Guid.CreateVersion7(timeProvider.GetUtcNow());
}
