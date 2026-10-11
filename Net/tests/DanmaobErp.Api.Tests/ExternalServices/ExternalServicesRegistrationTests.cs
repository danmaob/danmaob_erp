using DanmaobErp.Api.Tests.Errors.TestSupport;
using DanmaobErp.Application.Email;
using DanmaobErp.Application.Security;
using DanmaobErp.Application.Storage;
using DanmaobErp.Infrastructure.Email;
using DanmaobErp.Infrastructure.Security;
using DanmaobErp.Infrastructure.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobErp.Api.Tests.ExternalServices;

public sealed class ExternalServicesRegistrationTests
{
    [Fact]
    public void Development_RegistersLocalImplementations()
    {
        using var factory = TestApi.Create();
        Assert.IsType<FolderEmailSender>(factory.Services.GetRequiredService<IEmailSender>());
        Assert.IsType<LocalFileStorage>(factory.Services.GetRequiredService<IFileStorage>());
        Assert.IsType<DevelopmentHumanVerifier>(factory.Services.GetRequiredService<IHumanVerifier>());
    }

    [Fact]
    public void OtherEnvironments_RejectEveryHumanVerification()
    {
        using var baseFactory = TestApi.Create();
        using var factory = baseFactory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
        });
        Assert.IsType<RejectingHumanVerifier>(factory.Services.GetRequiredService<IHumanVerifier>());
    }
}
