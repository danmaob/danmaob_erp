using System.Reflection;
using NetArchTest.Rules;

namespace DanmaobErp.ArchitectureTests;

public sealed class LayerDependencyTests
{
    private static void AssertHasNoDependencies(Assembly assembly, params string[] forbiddenNamespaces)
    {
        var result = Types.InAssembly(assembly).ShouldNot().HaveDependencyOnAny(forbiddenNamespaces).GetResult();
        var failingTypes = result.FailingTypeNames ?? new string[0];
        Assert.True(result.IsSuccessful, $"Forbidden dependencies found in: {string.Join(", ", failingTypes)}");
    }

    [Fact]
    public void Domain_DoesNotDependOnOuterLayersOrFrameworks()
    {
        AssertHasNoDependencies(DanmaobErp.Domain.AssemblyReference.Assembly, "DanmaobErp.Application", "DanmaobErp.Infrastructure", "DanmaobErp.Api", "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore");
    }

    [Fact]
    public void Application_DoesNotDependOnInfrastructureApiOrEntityFrameworkCore()
    {
        AssertHasNoDependencies(DanmaobErp.Application.AssemblyReference.Assembly, "DanmaobErp.Infrastructure", "DanmaobErp.Api", "Microsoft.EntityFrameworkCore");
    }

    [Fact]
    public void Infrastructure_DoesNotDependOnApi()
    {
        AssertHasNoDependencies(DanmaobErp.Infrastructure.AssemblyReference.Assembly, "DanmaobErp.Api");
    }
}
