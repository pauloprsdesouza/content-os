namespace ContentOS.Application.Content.Ports;

public interface IContentPlacementQuery
{
    Task<bool> ReferencesAnyVersionAsync(
        IReadOnlyCollection<Guid> contentVersionIds,
        CancellationToken cancellationToken = default);
}
