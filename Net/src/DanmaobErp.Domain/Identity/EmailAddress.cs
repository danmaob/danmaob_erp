namespace DanmaobErp.Domain.Identity;

public static class EmailAddress
{
    public static string Normalize(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        return email.Trim().ToUpperInvariant();
    }
}
