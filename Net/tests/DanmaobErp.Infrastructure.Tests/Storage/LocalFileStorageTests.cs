using DanmaobErp.Infrastructure.Storage;

namespace DanmaobErp.Infrastructure.Tests.Storage;

public sealed class LocalFileStorageTests
{
    private static LocalFileStorage CreateStorage()
    {
        var root = Path.Combine(Path.GetTempPath(), "danmaob-erp-tests", Guid.NewGuid().ToString());
        return new LocalFileStorage(new FileStorageSettings(root));
    }

    [Fact]
    public async Task SaveAndOpen_ReturnsSameContent()
    {
        var storage = CreateStorage();
        var key = "tenants/company-a/logos/logo.png";
        var bytes = new byte[] { 1, 2, 3 };
        await storage.SaveAsync(key, new MemoryStream(bytes), CancellationToken.None);
        var result = await storage.ExistsAsync(key, CancellationToken.None);
        Assert.True(result);
        await using var stream = await storage.OpenReadAsync(key, CancellationToken.None);
        using var copy = new MemoryStream();
        await stream.CopyToAsync(copy);
        Assert.Equal(bytes, copy.ToArray());
    }

    [Theory]
    [InlineData("../outside.txt")]
    [InlineData("/etc/passwd")]
    [InlineData("tenants/../../outside.txt")]
    [InlineData("tenants/a b.txt")]
    [InlineData("")]
    public async Task InvalidKey_IsRejected(string key)
    {
        var storage = CreateStorage();
        await Assert.ThrowsAsync<ArgumentException>(async () => await storage.ExistsAsync(key, CancellationToken.None));
    }

    [Fact]
    public async Task OpenRead_Throws_WhenFileDoesNotExist()
    {
        var storage = CreateStorage();
        await Assert.ThrowsAsync<FileNotFoundException>(async () => await storage.OpenReadAsync("tenants/company-a/missing.txt", CancellationToken.None));
    }
}
