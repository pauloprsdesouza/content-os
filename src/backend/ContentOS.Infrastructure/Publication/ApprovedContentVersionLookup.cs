using ContentOS.Application.Publication.Ports;
using ContentOS.Domain.Content;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Publication;

public sealed class ApprovedContentVersionLookup(PlatformDbContext dbContext)
    : IApprovedContentVersionLookup
{
    public async Task<IReadOnlyDictionary<Guid, ApprovedContentVersionInfo>> GetApprovedAsync(
        IReadOnlyCollection<Guid> contentVersionIds,
        CancellationToken cancellationToken = default)
    {
        if (contentVersionIds.Count == 0)
        {
            return new Dictionary<Guid, ApprovedContentVersionInfo>();
        }

        var ids = contentVersionIds.Distinct().ToArray();
        var rows = await dbContext.Set<ContentVersion>()
            .AsNoTracking()
            .Where(version =>
                ids.Contains(version.Id)
                && version.Status == ContentVersionStatus.Approved)
            .Select(version => new ApprovedContentVersionInfo(
                version.Id,
                version.BodyMarkdown))
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(row => row.Id);
    }
}
