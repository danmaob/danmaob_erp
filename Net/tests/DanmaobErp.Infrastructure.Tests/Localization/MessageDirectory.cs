namespace DanmaobErp.Infrastructure.Tests.Localization;

public static class MessageDirectory
{
    public static string Create(string spanishJson)
    {
        var path = Path.Combine(Path.GetTempPath(), "danmaob-erp-tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(path);
        File.WriteAllText(Path.Combine(path, "es.json"), spanishJson);
        return path;
    }
}
