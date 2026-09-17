using ContentOS.Application.Blobs;
using ContentOS.Domain.Identity;
using NetArchTest.Rules;
using Shouldly;

namespace ContentOS.ArchitectureTests;

public sealed class ArchitectureDependencyTests
{
    [Fact]
    public void Domain_must_not_depend_on_outer_layers()
    {
        var result = Types.InAssembly(typeof(CapabilityNames).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "ContentOS.Application",
                "ContentOS.Contracts",
                "ContentOS.Infrastructure",
                "ContentOS.Api")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Application_must_not_depend_on_outer_layers()
    {
        var result = Types.InAssembly(typeof(IBlobStore).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "ContentOS.Contracts",
                "ContentOS.Infrastructure",
                "ContentOS.Api")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }
}
