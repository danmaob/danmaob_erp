namespace DanmaobErp.ArchitectureTests;

public static class SolutionPaths
{
    public static string GetSolutionDirectory()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var slnxPath = Path.Combine(current.FullName, "DanmaobErp.slnx");
            if (File.Exists(slnxPath))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("DanmaobErp.slnx was not found in any parent directory of the test output directory.");
    }

    public static string GetProjectFilePath(string projectName)
    {
        var path = Path.Combine(GetSolutionDirectory(), "src", projectName, projectName + ".csproj");

        if (File.Exists(path) is false)
        {
            throw new FileNotFoundException("Project file not found: " + path, path);
        }

        return path;
    }
}
