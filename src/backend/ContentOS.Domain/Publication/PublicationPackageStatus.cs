namespace ContentOS.Domain.Publication;

public enum PublicationPackageStatus
{
    Requested = 0,
    Building = 1,
    Validating = 2,
    ReadyForExport = 3,
    Exported = 4,
    PublishedConfirmed = 5
}
