namespace ContentOS.Application.Persistence;

public interface IChangeCommitter
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
