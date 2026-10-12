using DanmaobErp.Application.Identity;
using DanmaobErp.Domain.Auditing;

namespace DanmaobErp.Infrastructure.Tests.Persistence;

public sealed class FixedCurrentActor : ICurrentActor
{
    public ActorType ActorType { get; }
    public Guid? ActorId { get; }

    public FixedCurrentActor(ActorType actorType, Guid? actorId)
    {
        ActorType = actorType;
        ActorId = actorId;
    }
}
