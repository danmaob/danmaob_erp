namespace DanmaobErp.Infrastructure.Localization;

public sealed class LocalizationSettings
{
    public string Path { get; }
    public string DefaultCulture { get; }
    public IReadOnlyList<string> SupportedCultures { get; }

    public LocalizationSettings(string path, string defaultCulture, IReadOnlyList<string> supportedCultures)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultCulture);
        Path = path;
        DefaultCulture = defaultCulture;
        SupportedCultures = supportedCultures;
    }
}
