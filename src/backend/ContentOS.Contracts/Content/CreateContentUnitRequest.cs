namespace ContentOS.Contracts.Content;

public sealed record CreateContentUnitRequest(
    string Title,
    string? Brief,
    string Format,
    bool QueueGeneration = true,
    IReadOnlyList<string>? CitationContentHashes = null,
    Guid? ProductId = null);
