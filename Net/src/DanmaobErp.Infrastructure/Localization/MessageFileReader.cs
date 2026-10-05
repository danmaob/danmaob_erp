using System.Text.Json;

namespace DanmaobErp.Infrastructure.Localization;

public static class MessageFileReader
{
    public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ReadDirectory(string directoryPath)
    {
        if (Directory.Exists(directoryPath) is false)
        {
            throw new DirectoryNotFoundException($"The message dictionary directory '{directoryPath}' was not found.");
        }

        var result = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var filePath in Directory.GetFiles(directoryPath, "*.json"))
        {
            var culture = Path.GetFileNameWithoutExtension(filePath);
            var text = File.ReadAllText(filePath);
            var messages = JsonSerializer.Deserialize<Dictionary<string, string>>(text);
            if (messages is null)
            {
                throw new JsonException($"The message dictionary '{filePath}' is empty.");
            }

            result[culture] = messages;
        }

        return result;
    }
}
