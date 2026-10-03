namespace DanmaobErp.Api.Configuration;

public static class RequiredConfigurationKeys
{
    public const string CorsAllowedOrigins = "Cors:AllowedOrigins";

    public static IReadOnlyList<string> All { get; } = new[] { CorsAllowedOrigins };
}
