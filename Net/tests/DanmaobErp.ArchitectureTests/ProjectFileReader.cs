using System.Xml.Linq;

namespace DanmaobErp.ArchitectureTests;

public static class ProjectFileReader
{
    private static List<string> ReadIncludeValues(string projectName, string elementLocalName)
    {
        var path = SolutionPaths.GetProjectFilePath(projectName);
        var document = XDocument.Load(path);
        var values = new List<string>();

        foreach (var element in document.Descendants())
        {
            if (string.Equals(element.Name.LocalName, elementLocalName, StringComparison.Ordinal) is false)
            {
                continue;
            }

            var includeAttribute = element.Attribute("Include");
            if (includeAttribute is null)
            {
                throw new InvalidOperationException($"Element {elementLocalName} has no Include attribute in {path}");
            }

            values.Add(includeAttribute.Value);
        }

        return values;
    }

    public static IReadOnlyList<string> GetProjectReferenceNames(string projectName)
    {
        var values = ReadIncludeValues(projectName, "ProjectReference");
        var transformed = values.Select(v => Path.GetFileNameWithoutExtension(v.Replace("\\", "/"))).OrderBy(name => name, StringComparer.Ordinal).ToList();
        return transformed;
    }

    public static IReadOnlyList<string> GetPackageReferenceNames(string projectName)
    {
        var values = ReadIncludeValues(projectName, "PackageReference");
        return values.OrderBy(name => name, StringComparer.Ordinal).ToList();
    }
}
