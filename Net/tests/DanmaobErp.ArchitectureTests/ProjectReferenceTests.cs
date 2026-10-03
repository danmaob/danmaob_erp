namespace DanmaobErp.ArchitectureTests;

public sealed class ProjectReferenceTests
{
    [Fact]
    public void Domain_HasNoProjectReferences()
    {
        var projectReferences = ProjectFileReader.GetProjectReferenceNames("DanmaobErp.Domain");
        Assert.Empty(projectReferences);
    }

    [Fact]
    public void Application_ReferencesOnlyDomain()
    {
        var projectReferences = ProjectFileReader.GetProjectReferenceNames("DanmaobErp.Application");
        Assert.Equal(new[] { "DanmaobErp.Domain" }, projectReferences);
    }

    [Fact]
    public void Infrastructure_ReferencesOnlyApplicationAndDomain()
    {
        var projectReferences = ProjectFileReader.GetProjectReferenceNames("DanmaobErp.Infrastructure");
        Assert.Equal(new[] { "DanmaobErp.Application", "DanmaobErp.Domain" }, projectReferences);
    }

    [Fact]
    public void Api_ReferencesOnlyApplicationAndInfrastructure()
    {
        var projectReferences = ProjectFileReader.GetProjectReferenceNames("DanmaobErp.Api");
        Assert.Equal(new[] { "DanmaobErp.Application", "DanmaobErp.Infrastructure" }, projectReferences);
    }
}
