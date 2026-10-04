namespace DanmaobErp.Domain.Common;

public abstract class SoftDeletableEntity : Entity
{
    public bool IsActive { get; private set; } = true;

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Reactivate()
    {
        IsActive = true;
    }
}
