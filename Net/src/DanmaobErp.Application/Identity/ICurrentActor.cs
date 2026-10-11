using DanmaobErp.Domain.Auditing;

namespace DanmaobErp.Application.Identity;

public interface ICurrentActor
{
    public ActorType ActorType { get; }
    public Guid? ActorId { get; }
}
