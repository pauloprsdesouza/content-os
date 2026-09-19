using ContentOS.Domain.Publication;

namespace ContentOS.Application.Publication.ListPackages;

public sealed record ListPublicationPackagesResult(IReadOnlyList<PublicationPackage> Packages);
