using System.Text.RegularExpressions;
using DanmaobErp.Application.Storage;

namespace DanmaobErp.Infrastructure.Storage;

public sealed class LocalFileStorage : IFileStorage
{
    private static readonly Regex KeyPattern = new Regex("^[A-Za-z0-9._-]+(/[A-Za-z0-9._-]+)*$");
    private readonly string _rootPath;

    public LocalFileStorage(FileStorageSettings settings)
    {
        _rootPath = Path.GetFullPath(settings.RootPath);
    }

    public async Task SaveAsync(string key, Stream content, CancellationToken cancellationToken)
    {
        var path = ResolvePath(key);
        var directory = Path.GetDirectoryName(path);
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }
        await using var file = File.Create(path);
        await content.CopyToAsync(file, cancellationToken);
    }

    public Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken)
    {
        var path = ResolvePath(key);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("The file was not found.", key);
        }
        return Task.FromResult<Stream>(File.OpenRead(path));
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken)
    {
        return Task.FromResult(File.Exists(ResolvePath(key)));
    }

    private string ResolvePath(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || Path.IsPathRooted(key) || key.Contains("..") || key.Contains('\\') || KeyPattern.IsMatch(key) is false)
        {
            throw new ArgumentException("The file key is not valid.", nameof(key));
        }
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, key));
        if (fullPath.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.Ordinal) is false)
        {
            throw new ArgumentException("The file key is not valid.", nameof(key));
        }
        return fullPath;
    }
}
