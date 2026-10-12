using System.Security.Claims;
using DanmaobErp.Api.Identity;
using DanmaobErp.Application.Identity;
using DanmaobErp.Domain.Auditing;
using Microsoft.AspNetCore.Http;

namespace DanmaobErp.Api.Tests.Identity;

public sealed class HttpCurrentActorTests
{
    private static HttpCurrentActor CreateSut(string? claimValue)
    {
        var httpContext = new DefaultHttpContext();
        if (claimValue is not null)
        {
            var identity = new ClaimsIdentity(new Claim[] { new Claim(ActorClaimTypes.UserId, claimValue) }, "Test");
            httpContext.User = new ClaimsPrincipal(identity);
        }
        var accessor = new HttpContextAccessor();
        accessor.HttpContext = httpContext;
        return new HttpCurrentActor(accessor);
    }

    [Fact]
    public void ValidUserClaim_IsUserWithItsId()
    {
        var userId = Guid.CreateVersion7();
        var sut = CreateSut(userId.ToString());
        Assert.Equal(ActorType.User, sut.ActorType);
        Assert.Equal(userId, sut.ActorId);
    }

    [Fact]
    public void RequestWithoutUserClaim_IsAnonymous()
    {
        var sut = CreateSut(null);
        Assert.Equal(ActorType.Anonymous, sut.ActorType);
        Assert.Null(sut.ActorId);
    }

    [Fact]
    public void InvalidUserClaim_IsAnonymous()
    {
        var sut = CreateSut("not-a-guid");
        Assert.Equal(ActorType.Anonymous, sut.ActorType);
        Assert.Null(sut.ActorId);
    }

    [Fact]
    public void NoHttpContext_IsSystem()
    {
        var sut = new HttpCurrentActor(new HttpContextAccessor());
        Assert.Equal(ActorType.System, sut.ActorType);
        Assert.Null(sut.ActorId);
    }
}
