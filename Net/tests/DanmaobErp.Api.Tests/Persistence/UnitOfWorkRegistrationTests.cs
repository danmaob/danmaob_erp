using DanmaobErp.Api.Tests.Errors.TestSupport;
using DanmaobErp.Application.Events;
using DanmaobErp.Application.Persistence;
using DanmaobErp.Infrastructure.Events;
using DanmaobErp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobErp.Api.Tests.Persistence;

public sealed class UnitOfWorkRegistrationTests
{
    [Fact]
    public void UnitOfWork_IsResolvedWithThePendingEventsOfTheRequest()
    {
        using var factory = TestApi.Create();
        using var scope = factory.Services.CreateScope();
        Assert.IsType<UnitOfWork>(scope.ServiceProvider.GetRequiredService<IUnitOfWork>());
        Assert.Same(scope.ServiceProvider.GetRequiredService<PendingBusinessEvents>(), scope.ServiceProvider.GetRequiredService<IPendingBusinessEvents>());
    }
}
