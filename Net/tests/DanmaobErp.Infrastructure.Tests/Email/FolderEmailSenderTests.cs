using DanmaobErp.Application.Email;
using DanmaobErp.Infrastructure.Email;

namespace DanmaobErp.Infrastructure.Tests.Email;

public sealed class FolderEmailSenderTests
{
    [Fact]
    public async Task SendAsync_WritesEmlFileWithRecipientAndSubject()
    {
        var directory = Path.Combine(Path.GetTempPath(), "danmaob-erp-tests", Guid.NewGuid().ToString());
        var sender = new FolderEmailSender(new EmailSettings(directory, "no-responder@danmaob.com.mx"));
        var message = new EmailMessage("cliente@example.com", "Bienvenida", "<p>Hola</p>", "Hola");
        await sender.SendAsync(message, CancellationToken.None);
        var files = Directory.GetFiles(directory, "*.eml");
        var file = Assert.Single(files);
        var text = File.ReadAllText(file);
        Assert.Contains("To: cliente@example.com", text);
        Assert.Contains("Subject: Bienvenida", text);
    }
}
