using ContentOS.Application.Knowledge.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Knowledge;
using ContentOS.SharedKernel;

namespace ContentOS.Application.Knowledge.Sources.Create;

public sealed class CreateSourceHandler(
    ISourceRepository sources,
    IIdGenerator ids,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<CreateSourceResult> HandleAsync(
        CreateSourceCommand command,
        CancellationToken cancellationToken = default)
    {
        SourceUri location;
        try
        {
            location = SourceUri.Create(command.Location);
        }
        catch (ArgumentException)
        {
            return CreateSourceResult.InvalidUri();
        }

        if (await sources.CanonicalUriExistsAsync(location, cancellationToken))
        {
            return CreateSourceResult.Duplicate();
        }

        var now = clock.GetUtcNow();
        var source = Source.Create(
            ids.NewId(),
            location,
            command.Kind,
            command.DisplayName,
            now);

        sources.Add(source);
        await changes.CommitAsync(cancellationToken);
        return CreateSourceResult.Created(source.Id);
    }
}
