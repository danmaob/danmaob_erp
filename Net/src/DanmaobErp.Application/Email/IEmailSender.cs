namespace DanmaobErp.Application.Email;

public interface IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
