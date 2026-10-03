namespace DanmaobErp.Api.Configuration;

public static class RequiredConfiguration
{
    public static string ToEnvironmentVariableName(string key)
    {
        return key.Replace(":", "__");
    }

    public static void EnsurePresent(IConfiguration configuration, IReadOnlyList<string> keys)
    {
        var missing = new List<string>();

        foreach (var key in keys)
        {
            var value = configuration[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                missing.Add(ToEnvironmentVariableName(key));
            }
        }

        if (!missing.Any())
        {
            return;
        }

        throw new InvalidOperationException($"Missing required configuration. Set these environment variables: {string.Join(", ", missing)}");
    }
}
