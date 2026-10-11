using DanmaobErp.Infrastructure.Security;

namespace DanmaobErp.Infrastructure.Tests.Security;

public sealed class HumanVerifierTests
{
    [Fact]
    public async Task DevelopmentVerifier_AcceptsToken()
    {
        var verifier = new DevelopmentHumanVerifier();
        var result = await verifier.VerifyAsync("token", null, CancellationToken.None);
        Assert.True(result);
    }

    [Fact]
    public async Task DevelopmentVerifier_RejectsEmptyToken()
    {
        var verifier = new DevelopmentHumanVerifier();
        var result = await verifier.VerifyAsync("", null, CancellationToken.None);
        Assert.False(result);
    }

    [Fact]
    public async Task RejectingVerifier_RejectsEveryToken()
    {
        var verifier = new RejectingHumanVerifier();
        var result = await verifier.VerifyAsync("token", "127.0.0.1", CancellationToken.None);
        Assert.False(result);
    }
}
