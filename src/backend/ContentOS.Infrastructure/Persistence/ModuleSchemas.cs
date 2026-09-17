namespace ContentOS.Infrastructure.Persistence;

public static class ModuleSchemas
{
    public const string Identity = "identity";
    public const string Knowledge = "knowledge";
    public const string Research = "research";
    public const string Content = "content";
    public const string Catalog = "catalog";
    public const string Publication = "publication";
    public const string Commerce = "commerce";
    public const string Learning = "learning";
    public const string Wolverine = "wolverine";

    public static IReadOnlyList<string> All { get; } =
    [
        Identity,
        Knowledge,
        Research,
        Content,
        Catalog,
        Publication,
        Commerce,
        Learning,
        Wolverine
    ];
}
