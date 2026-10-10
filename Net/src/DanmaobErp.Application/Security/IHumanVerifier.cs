namespace DanmaobErp.Application.Security;

public interface IHumanVerifier
{
    public Task<bool> VerifyAsync(string? token, string? remoteIp, CancellationToken cancellationToken);
}
