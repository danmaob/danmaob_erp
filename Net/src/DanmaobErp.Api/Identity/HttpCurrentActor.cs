using DanmaobErp.Application.Identity;
using DanmaobErp.Domain.Auditing;

namespace DanmaobErp.Api.Identity;

public sealed class HttpCurrentActor : ICurrentActor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentActor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ActorType ActorType
    {
        get
        {
            if (_httpContextAccessor.HttpContext is null)
            {
                return ActorType.System;
            }

            if (ResolveUserId() is not null)
            {
                return ActorType.User;
            }

            return ActorType.Anonymous;
        }
    }

    public Guid? ActorId
    {
        get
        {
            return ResolveUserId();
        }
    }

    private Guid? ResolveUserId()
    {
        var claimValue = _httpContextAccessor.HttpContext?.User.FindFirst(ActorClaimTypes.UserId)?.Value;

        if (Guid.TryParse(claimValue, out var userId) && userId != Guid.Empty)
        {
            return userId;
        }

        return null;
    }
}
