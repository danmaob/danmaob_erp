using System.Security.Claims;
using DanmaobErp.Api.Tenancy;
using DanmaobErp.Application.Tenancy;
using Microsoft.AspNetCore.Http;

namespace DanmaobErp.Api.Tests.Tenancy;

public sealed class HttpTenantContextTests
{
    private static HttpTenantContext CreateSut(string? claimValue)
    {
        var httpContext = new DefaultHttpContext();
        if (claimValue is not null)
        {
            var identity = new ClaimsIdentity(
                [new Claim(TenantClaimTypes.TenantId, claimValue)], "Test");
            httpContext.User = new ClaimsPrincipal(identity);
        }

        var accessor = new HttpContextAccessor { HttpContext = httpContext };
        return new HttpTenantContext(accessor);
    }

    [Fact]
    public void RequireTenantId_ReturnsTenant_WhenClaimIsValid()
    {
        var tenantId = Guid.CreateVersion7();
        var sut = CreateSut(tenantId.ToString());
        Assert.Equal(tenantId, sut.RequireTenantId());
    }

    [Fact]
    public void RequireTenantId_Throws_WhenClaimIsMissing()
    {
        var sut = CreateSut(null);
        Assert.Throws<TenantNotResolvedException>(() => sut.RequireTenantId());
    }

    [Fact]
    public void RequireTenantId_Throws_WhenClaimIsNotAGuid()
    {
        var sut = CreateSut("not-a-guid");
        Assert.Throws<TenantNotResolvedException>(() => sut.RequireTenantId());
    }

    [Fact]
    public void RequireTenantId_Throws_WhenClaimIsEmptyGuid()
    {
        var sut = CreateSut(Guid.Empty.ToString());
        Assert.Throws<TenantNotResolvedException>(() => sut.RequireTenantId());
    }

    [Fact]
    public void RequireTenantId_Throws_WhenThereIsNoHttpContext()
    {
        var accessor = new HttpContextAccessor();
        var sut = new HttpTenantContext(accessor);
        Assert.Throws<TenantNotResolvedException>(() => sut.RequireTenantId());
    }
}
