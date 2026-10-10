namespace DanmaobErp.Infrastructure.Email;

public sealed class EmailSettings
{
    public string OutboxPath { get; }
    public string From { get; }

    public EmailSettings(string outboxPath, string from)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outboxPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(from);
        OutboxPath = outboxPath;
        From = from;
    }
}
