using System.Net.Mail;
using DanmaobErp.Application.Email;

namespace DanmaobErp.Infrastructure.Email;

public sealed class FolderEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;

    public FolderEmailSender(EmailSettings settings)
    {
        _settings = settings;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_settings.OutboxPath);

        using var mailMessage = new MailMessage();
        mailMessage.From = new MailAddress(_settings.From);
        mailMessage.To.Add(message.To);
        mailMessage.Subject = message.Subject;
        mailMessage.Body = message.HtmlBody;
        mailMessage.IsBodyHtml = true;
        mailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message.TextBody, null, "text/plain"));

        using var client = new SmtpClient();
        client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
        client.PickupDirectoryLocation = _settings.OutboxPath;

        await client.SendMailAsync(mailMessage, cancellationToken);
    }
}
