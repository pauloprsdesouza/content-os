using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;

namespace ContentOS.ArchitectureTests;

public sealed class SourceConventionTests
{
    [Fact]
    public void Hand_written_files_must_contain_one_matching_type()
    {
        var violations = new List<string>();

        foreach (var file in GetHandWrittenSourceFiles())
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));
            var root = syntaxTree.GetRoot();
            var declarations = root.DescendantNodes()
                .Where(node =>
                    node is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax)
                .ToArray();

            if (declarations.Length != 1)
            {
                violations.Add(
                    $"{Path.GetRelativePath(GetRepositoryRoot(), file)} declares {declarations.Length} types.");
                continue;
            }

            var declaredName = declarations[0] switch
            {
                BaseTypeDeclarationSyntax type => type.Identifier.ValueText,
                DelegateDeclarationSyntax type => type.Identifier.ValueText,
                _ => string.Empty
            };

            if (!string.Equals(
                    declaredName,
                    Path.GetFileNameWithoutExtension(file),
                    StringComparison.Ordinal))
            {
                violations.Add(
                    $"{Path.GetRelativePath(GetRepositoryRoot(), file)} declares {declaredName}.");
            }
        }

        violations.ShouldBeEmpty();
    }

    [Fact]
    public void Hand_written_types_must_use_file_scoped_namespaces()
    {
        var violations = GetHandWrittenSourceFiles()
            .Where(file =>
            {
                var root = CSharpSyntaxTree.ParseText(File.ReadAllText(file)).GetRoot();
                return root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Any();
            })
            .Select(file => Path.GetRelativePath(GetRepositoryRoot(), file))
            .ToArray();

        violations.ShouldBeEmpty();
    }

    private static IEnumerable<string> GetHandWrittenSourceFiles()
    {
        var repositoryRoot = GetRepositoryRoot();

        return Directory.EnumerateFiles(repositoryRoot, "*.cs", SearchOption.AllDirectories)
            .Where(file => !ContainsDirectory(file, "bin"))
            .Where(file => !ContainsDirectory(file, "obj"))
            .Where(file => !file.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase))
            .Where(file => !file.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase))
            .Where(file => !string.Equals(
                Path.GetFileName(file),
                "Program.cs",
                StringComparison.OrdinalIgnoreCase))
            .Where(file => !string.Equals(
                Path.GetFileName(file),
                "GlobalUsings.cs",
                StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsDirectory(string path, string directoryName)
    {
        var marker = $"{Path.DirectorySeparatorChar}{directoryName}{Path.DirectorySeparatorChar}";
        return path.Contains(marker, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "src", "backend", "ContentOS.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the Content OS repository root.");
    }
}
