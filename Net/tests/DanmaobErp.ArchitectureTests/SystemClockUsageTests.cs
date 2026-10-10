namespace DanmaobErp.ArchitectureTests;

public sealed class SystemClockUsageTests
{
    private static readonly string[] ForbiddenTokens = { "DateTime.Now", "DateTime.UtcNow", "DateTimeOffset.Now", "DateTimeOffset.UtcNow" };

    private static List<string> FindSystemClockUsages(string directory)
    {
        var usages = new List<string>();
        foreach (var file in Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(directory, file);
            if (relativePath.Contains("/obj/") || relativePath.Contains("/bin/") || relativePath.StartsWith("obj/") || relativePath.StartsWith("bin/"))
            {
                continue;
            }

            var lines = File.ReadAllLines(file);
            for (int index = 0; index < lines.Length; index++)
            {
                if (ForbiddenTokens.Any(token => lines[index].Contains(token)))
                {
                    usages.Add(relativePath + ":" + (index + 1));
                }
            }
        }

        return usages;
    }

    [Fact]
    public void Source_DoesNotUseSystemClock()
    {
        var sourceDirectory = Path.Combine(SolutionPaths.GetSolutionDirectory(), "src");
        var usages = FindSystemClockUsages(sourceDirectory);
        Assert.Empty(usages);
    }

    [Fact]
    public void Check_DetectsSystemClockUsage()
    {
        var directory = Path.Combine(Path.GetTempPath(), "danmaob-erp-tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "Sample.cs"), "var now = DateTime.UtcNow;");
        var usages = FindSystemClockUsages(directory);
        Assert.Single(usages);
    }
}
