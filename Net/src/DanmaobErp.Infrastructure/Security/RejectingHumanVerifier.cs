using DanmaobErp.Application.Security;

namespace DanmaobErp.Infrastructure.Security;

public sealed class RejectingHumanVerifier : IHumanVerifier
{
    public Task<bool> VerifyAsync(string? token, string? remoteIp, CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }
}
