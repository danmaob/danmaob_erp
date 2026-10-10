namespace DanmaobErp.Application.Storage;

public interface IFileStorage
{
    public Task SaveAsync(string key, Stream content, CancellationToken cancellationToken);
    public Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken);
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken);
}
