namespace DanmaobErp.Infrastructure.Storage;

public sealed class FileStorageSettings
{
    public string RootPath { get; }

    public FileStorageSettings(string rootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
        RootPath = rootPath;
    }
}
