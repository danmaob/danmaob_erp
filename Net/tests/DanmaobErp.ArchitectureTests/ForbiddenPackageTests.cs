namespace DanmaobErp.ArchitectureTests;

public sealed class ForbiddenPackageTests
{
    [Theory]
    [InlineData("DanmaobErp.Domain")]
    [InlineData("DanmaobErp.Application")]
    [InlineData("DanmaobErp.Infrastructure")]
    [InlineData("DanmaobErp.Api")]
    public void SourceProject_DoesNotReferenceMediatR(string projectName)
    {
        var packages = ProjectFileReader.GetPackageReferenceNames(projectName);
        Assert.DoesNotContain(packages, p => p.StartsWith("MediatR", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("DanmaobErp.Domain")]
    [InlineData("DanmaobErp.Application")]
    public void InnerLayer_DoesNotReferenceEntityFrameworkCoreOrAspNetCore(string projectName)
    {
        var packages = ProjectFileReader.GetPackageReferenceNames(projectName);
        Assert.DoesNotContain(packages, p => p.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.OrdinalIgnoreCase) || p.StartsWith("Microsoft.AspNetCore", StringComparison.OrdinalIgnoreCase));
    }
}
