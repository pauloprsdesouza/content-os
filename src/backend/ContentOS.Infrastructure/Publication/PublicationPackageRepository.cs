using ContentOS.Application.Publication.Ports;
using ContentOS.Domain.Publication;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Publication;

public sealed class PublicationPackageRepository(PlatformDbContext dbContext)
    : IPublicationPackageRepository
{
    public Task<PublicationPackage?> GetByIdAsync(
        Guid packageId,
        CancellationToken cancellationToken = default) =>
        dbContext.Set<PublicationPackage>()
            .FirstOrDefaultAsync(package => package.Id == packageId, cancellationToken);

    public async Task<IReadOnlyList<PublicationPackage>> ListByEditionAsync(
        Guid editionId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<PublicationPackage>()
            .AsNoTracking()
            .Where(package => package.EditionId == editionId)
            .OrderByDescending(package => package.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public void Add(PublicationPackage package) =>
        dbContext.Set<PublicationPackage>().Add(package);
}
