namespace ContentOS.Application.Publication.Ports;

public interface IApprovedContentVersionLookup
{
    Task<IReadOnlyDictionary<Guid, ApprovedContentVersionInfo>> GetApprovedAsync(
        IReadOnlyCollection<Guid> contentVersionIds,
        CancellationToken cancellationToken = default);
}
